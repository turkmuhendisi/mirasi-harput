<?php

namespace MirasiHarput;

/**
 * Verilen şablon görseli üzerine ziyaretçi adını, tarihini ve mekan
 * bilgilerini bindirerek bir sertifika PNG dosyası üretir (GD kullanır).
 */
final class CertificateGenerator
{
    private string $basePath;

    public function __construct(string $basePath)
    {
        $this->basePath = rtrim($basePath, '/');
    }

    /**
     * @return string Üretilen PNG dosyasının tam yolu
     */
    public function generate(string $fullName, string $certificateCode, \DateTimeInterface $issuedAt): string
    {
        if (!\extension_loaded('gd')) {
            throw new \RuntimeException('GD eklentisi yüklü değil; sertifika üretilemiyor.');
        }

        $templatePath = $this->resolve(Env::get('CERT_TEMPLATE', 'assets/certificate-template.png'));
        if (!is_readable($templatePath)) {
            throw new \RuntimeException("Sertifika şablonu bulunamadı: {$templatePath}");
        }

        $image = $this->loadTemplateImage($templatePath);
        if ($image === false) {
            throw new \RuntimeException('Sertifika şablonu açılamadı. PNG veya JPEG olmalıdır.');
        }

        $width = imagesx($image);
        $height = imagesy($image);

        $gold = imagecolorallocate($image, 0xB2, 0x8A, 0x3C);
        $dark = imagecolorallocate($image, 0x4A, 0x3A, 0x1E);

        $fontRegular = $this->resolve(Env::get('CERT_FONT_REGULAR', 'assets/fonts/DejaVuSans.ttf'));
        $fontBold = $this->resolve(Env::get('CERT_FONT_BOLD', 'assets/fonts/DejaVuSans-Bold.ttf'));
        if (!is_readable($fontBold)) {
            $fontBold = $fontRegular;
        }

        $centerX = (int) ($width / 2);

        $this->centeredText($image, $fontBold, 26, (int) ($height * 0.34), $gold, 'KATILIM SERTİFİKASI', $centerX);
        $this->centeredText($image, $fontRegular, 16, (int) ($height * 0.42), $dark, 'Bu belge, aşağıda adı geçen ziyaretçinin', $centerX);

        $this->centeredText($image, $fontBold, 40, (int) ($height * 0.55), $dark, $fullName, $centerX);

        $line = 'Harput Kalesi ve Urartu Sarnıcı / Zindanı mekanlarını ziyaret ederek';
        $line2 = 'Miras\'ı Harput deneyimini tamamladığını onaylar.';
        $this->centeredText($image, $fontRegular, 16, (int) ($height * 0.66), $dark, $line, $centerX);
        $this->centeredText($image, $fontRegular, 16, (int) ($height * 0.71), $dark, $line2, $centerX);

        $dateText = 'Tarih: ' . $this->formatDate($issuedAt);
        $this->centeredText($image, $fontRegular, 14, (int) ($height * 0.82), $dark, $dateText, $centerX);
        $this->centeredText($image, $fontRegular, 12, (int) ($height * 0.87), $dark, 'Belge No: ' . $certificateCode, $centerX);

        $outputDir = $this->ensureWritableOutputDir();
        $outputPath = $outputDir . '/' . $certificateCode . '.png';

        $saved = @imagepng($image, $outputPath);
        imagedestroy($image);

        if (!$saved || !is_readable($outputPath)) {
            $writable = is_writable($outputDir) ? 'yes' : 'no';
            throw new \RuntimeException(
                "Sertifika dosyası kaydedilemedi: {$outputPath} (dir_writable={$writable})"
            );
        }

        return $outputPath;
    }

    private function ensureWritableOutputDir(): string
    {
        $outputDir = $this->resolve('storage/certificates');

        if (!is_dir($outputDir)) {
            if (!@mkdir($outputDir, 0775, true) && !is_dir($outputDir)) {
                throw new \RuntimeException("Sertifika klasörü oluşturulamadı: {$outputDir}");
            }
        }

        if (!is_writable($outputDir)) {
            @chmod($outputDir, 0775);
        }

        if (!is_writable($outputDir)) {
            throw new \RuntimeException(
                "Sertifika klasörüne yazılamıyor: {$outputDir}. Sunucuda: chmod 775 storage/certificates"
            );
        }

        return $outputDir;
    }

    private function centeredText($image, string $fontPath, int $size, int $y, int $color, string $text, int $centerX): void
    {
        if (is_readable($fontPath)) {
            $box = imagettfbbox($size, 0, $fontPath, $text);
            $textWidth = abs($box[2] - $box[0]);
            $x = (int) ($centerX - $textWidth / 2);
            imagettftext($image, $size, 0, $x, $y, $color, $fontPath, $text);
            return;
        }

        // TTF yoksa dahili bitmap font ile yedek çizim (Türkçe karakterler bozulabilir).
        $glyphWidth = imagefontwidth(5);
        $x = (int) ($centerX - (strlen($text) * $glyphWidth) / 2);
        imagestring($image, 5, $x, $y, $text, $color);
    }

    private function formatDate(\DateTimeInterface $date): string
    {
        $months = [
            1 => 'Ocak', 2 => 'Şubat', 3 => 'Mart', 4 => 'Nisan',
            5 => 'Mayıs', 6 => 'Haziran', 7 => 'Temmuz', 8 => 'Ağustos',
            9 => 'Eylül', 10 => 'Ekim', 11 => 'Kasım', 12 => 'Aralık',
        ];
        $day = (int) $date->format('j');
        $month = $months[(int) $date->format('n')];
        $year = $date->format('Y');
        return "{$day} {$month} {$year}";
    }

    /**
     * Şablon PNG veya JPEG olabilir (.png uzantılı JPEG dosyaları da desteklenir).
     *
     * @return resource|false
     */
    private function loadTemplateImage(string $templatePath)
    {
        $bytes = @file_get_contents($templatePath);
        if ($bytes === false || $bytes === '') {
            return false;
        }

        $image = @imagecreatefromstring($bytes);
        if ($image !== false) {
            return $image;
        }

        $mime = null;
        if (\function_exists('finfo_open')) {
            $finfo = finfo_open(FILEINFO_MIME_TYPE);
            if ($finfo !== false) {
                $mime = finfo_buffer($finfo, $bytes);
                finfo_close($finfo);
            }
        }

        if ($mime === 'image/jpeg' || $mime === 'image/jpg') {
            return @imagecreatefromjpeg($templatePath);
        }
        if ($mime === 'image/png') {
            return @imagecreatefrompng($templatePath);
        }

        return false;
    }

    private function resolve(string $relativeOrAbsolute): string
    {
        if (str_starts_with($relativeOrAbsolute, '/')) {
            return $relativeOrAbsolute;
        }
        return $this->basePath . '/' . ltrim($relativeOrAbsolute, '/');
    }
}
