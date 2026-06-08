<?php

use MirasiHarput\Env;

$backendRoot = dirname(__DIR__);

// PSR-4 autoloader (composer yoksa da çalışır)
spl_autoload_register(static function (string $class) use ($backendRoot): void {
    $prefix = 'MirasiHarput\\';
    if (!str_starts_with($class, $prefix)) {
        return;
    }
    $relative = substr($class, strlen($prefix));
    $file = $backendRoot . '/src/' . str_replace('\\', '/', $relative) . '.php';
    if (is_readable($file)) {
        require $file;
    }
});

// composer bağımlılıkları (PHPMailer) varsa yükle
$composerAutoload = $backendRoot . '/vendor/autoload.php';
if (is_readable($composerAutoload)) {
    require $composerAutoload;
}

Env::load($backendRoot . '/.env');

return $backendRoot;
