<?php

use MirasiHarput\Database;
use MirasiHarput\Schema;

require dirname(__DIR__) . '/src/bootstrap.php';

header('Content-Type: application/json; charset=utf-8');

$status = ['success' => true, 'service' => 'mirasiharput-backend'];

try {
    $pdo = Database::connection();
    $pdo->query('SELECT 1');
    $status['database'] = 'ok';
    $status['certificates_table'] = Schema::certificatesTableExists($pdo) ? 'ok' : 'missing';
} catch (\Throwable $e) {
    $status['success'] = false;
    $status['database'] = 'error';
    $status['message'] = 'Veritabanı bağlantısı kurulamadı.';
}

echo json_encode($status, JSON_UNESCAPED_UNICODE);
