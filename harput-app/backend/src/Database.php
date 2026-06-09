<?php

namespace MirasiHarput;

use PDO;

final class Database
{
    private static ?PDO $connection = null;

    public static function connection(): PDO
    {
        if (self::$connection instanceof PDO) {
            return self::$connection;
        }

        $host = Env::require('DB_HOST');
        $port = Env::get('DB_PORT', '3306');
        $name = Env::require('DB_NAME');
        $user = Env::require('DB_USER');
        $password = Env::require('DB_PASSWORD');
        $charset = Env::get('DB_CHARSET', 'utf8mb4');

        $dsn = "mysql:host={$host};port={$port};dbname={$name};charset={$charset}";

        self::$connection = new PDO($dsn, $user, $password, [
            PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
            PDO::ATTR_DEFAULT_FETCH_MODE => PDO::FETCH_ASSOC,
            PDO::ATTR_EMULATE_PREPARES => false,
        ]);

        try {
            Schema::ensureTables(self::$connection);
        } catch (\Throwable $e) {
            error_log('[schema] ensureTables failed: ' . $e->getMessage());
        }

        return self::$connection;
    }
}
