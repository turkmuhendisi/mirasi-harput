<?php

use MirasiHarput\Database;

require dirname(__DIR__) . '/src/bootstrap.php';

header('Content-Type: application/json; charset=utf-8');

$status = ['success' => true, 'service' => 'mirasiharput-backend'];

try {
    Database::connection()->query('SELECT 1');
    $status['database'] = 'ok';
} catch (\Throwable $e) {
    $status['database'] = 'error';
}

echo json_encode($status, JSON_UNESCAPED_UNICODE);
