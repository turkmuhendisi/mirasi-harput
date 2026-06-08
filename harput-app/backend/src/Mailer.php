<?php

namespace MirasiHarput;

use PHPMailer\PHPMailer\PHPMailer;
use PHPMailer\PHPMailer\Exception as PHPMailerException;

final class Mailer
{
    public function sendCertificate(string $toEmail, string $toName, string $certificatePath): void
    {
        if (!class_exists(PHPMailer::class)) {
            throw new \RuntimeException('PHPMailer yüklü değil. "composer install" çalıştırın.');
        }

        $mail = new PHPMailer(true);

        try {
            $mail->isSMTP();
            $mail->Host = Env::require('MAIL_HOST');
            $mail->SMTPAuth = true;
            $mail->Username = Env::require('MAIL_USERNAME');
            $mail->Password = Env::require('MAIL_PASSWORD');
            $mail->Port = (int) Env::get('MAIL_PORT', '465');
            $mail->CharSet = 'UTF-8';

            $encryption = strtolower((string) Env::get('MAIL_ENCRYPTION', 'ssl'));
            if ($encryption === 'tls') {
                $mail->SMTPSecure = PHPMailer::ENCRYPTION_STARTTLS;
            } else {
                $mail->SMTPSecure = PHPMailer::ENCRYPTION_SMTPS;
            }

            $fromAddress = Env::get('MAIL_FROM_ADDRESS') ?: Env::require('MAIL_USERNAME');
            $fromName = Env::get('MAIL_FROM_NAME', "Miras'ı Harput");
            $mail->setFrom($fromAddress, $fromName);
            $mail->addAddress($toEmail, $toName);

            $mail->addAttachment($certificatePath, 'MirasiHarput-Sertifika.png');

            $mail->isHTML(true);
            $mail->Subject = "Miras'ı Harput - Ziyaret Sertifikanız";
            $mail->Body = $this->htmlBody($toName);
            $mail->AltBody = $this->textBody($toName);

            $mail->send();
        } catch (PHPMailerException $e) {
            throw new \RuntimeException('E-posta gönderilemedi: ' . $mail->ErrorInfo);
        }
    }

    private function htmlBody(string $name): string
    {
        $safeName = htmlspecialchars($name, ENT_QUOTES, 'UTF-8');
        return <<<HTML
<div style="font-family:Arial,sans-serif;color:#4A3A1E;line-height:1.6">
  <h2 style="color:#B28A3C">Tebrikler, {$safeName}!</h2>
  <p>Harput Kalesi ve Urartu Sarnıcı / Zindanı mekanlarını ziyaret ederek
  <strong>Miras'ı Harput</strong> deneyimini tamamladınız.</p>
  <p>Katılım sertifikanızı bu e-postanın ekinde bulabilirsiniz.</p>
  <p style="margin-top:24px">Harput'un binlerce yıllık mirasını keşfettiğiniz için teşekkür ederiz.</p>
  <p style="color:#B28A3C;font-weight:bold">Miras'ı Harput</p>
</div>
HTML;
    }

    private function textBody(string $name): string
    {
        return "Tebrikler, {$name}!\n\n"
            . "Harput Kalesi ve Urartu Sarnici / Zindani mekanlarini ziyaret ederek "
            . "Miras'i Harput deneyimini tamamladiniz.\n"
            . "Katilim sertifikaniz e-postanin ekindedir.\n\n"
            . "Miras'i Harput";
    }
}
