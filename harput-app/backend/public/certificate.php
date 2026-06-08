<?php

use MirasiHarput\CertificateGenerator;
use MirasiHarput\Database;
use MirasiHarput\Env;
use MirasiHarput\Mailer;

$backendRoot = require dirname(__DIR__) . '/src/bootstrap.php';

header('Content-Type: application/json; charset=utf-8');

function respond(int $status, array $payload): void
{
    http_response_code($status);
    echo json_encode($payload, JSON_UNESCAPED_UNICODE);
    exit;
}

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    respond(405, ['success' => false, 'message' => 'Yalnızca POST destekleniyor.']);
}

$raw = file_get_contents('php://input');
$data = json_decode($raw, true);
if (!is_array($data)) {
    $data = $_POST;
}

// Opsiyonel API anahtarı kontrolü
$expectedKey = Env::get('API_KEY');
if ($expectedKey !== null && $expectedKey !== '') {
    $providedKey = $_SERVER['HTTP_X_API_KEY'] ?? ($data['apiKey'] ?? '');
    if (!hash_equals($expectedKey, (string) $providedKey)) {
        respond(401, ['success' => false, 'message' => 'Yetkisiz istek.']);
    }
}

$fullName = trim((string) ($data['fullName'] ?? ''));
$email = trim((string) ($data['email'] ?? ''));

if ($fullName === '' || mb_strlen($fullName) < 3) {
    respond(422, ['success' => false, 'message' => 'Lütfen geçerli bir ad soyad girin.']);
}
if (!filter_var($email, FILTER_VALIDATE_EMAIL)) {
    respond(422, ['success' => false, 'message' => 'Lütfen geçerli bir e-posta adresi girin.']);
}

try {
    $issuedAt = new DateTimeImmutable('now');
    $certificateCode = 'MH-' . strtoupper(bin2hex(random_bytes(4)));

    $pdo = Database::connection();
    $stmt = $pdo->prepare(
        'INSERT INTO certificates (full_name, email, certificate_code, locations, email_sent, created_at)
         VALUES (:full_name, :email, :code, :locations, 0, :created_at)'
    );
    $stmt->execute([
        ':full_name' => $fullName,
        ':email' => $email,
        ':code' => $certificateCode,
        ':locations' => 'harput_kalesi,urartu_sarnici_zindani',
        ':created_at' => $issuedAt->format('Y-m-d H:i:s'),
    ]);
    $recordId = (int) $pdo->lastInsertId();

    $generator = new CertificateGenerator($backendRoot);
    $certificatePath = $generator->generate($fullName, $certificateCode, $issuedAt);

    $mailer = new Mailer();
    $mailer->sendCertificate($email, $fullName, $certificatePath);

    $pdo->prepare('UPDATE certificates SET email_sent = 1 WHERE id = :id')
        ->execute([':id' => $recordId]);

    respond(200, [
        'success' => true,
        'message' => 'Sertifikanız e-posta adresinize gönderildi.',
        'certificateCode' => $certificateCode,
    ]);
} catch (\Throwable $e) {
    error_log('[certificate] ' . $e->getMessage());
    respond(500, [
        'success' => false,
        'message' => 'Sertifika gönderilirken bir hata oluştu. Lütfen daha sonra tekrar deneyin.',
    ]);
}
