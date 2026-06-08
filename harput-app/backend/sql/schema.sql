-- Miras'ı Harput - sertifika kayıtları tablosu
-- Hostinger phpMyAdmin üzerinde u824624299_harput veritabanında çalıştırın.

CREATE TABLE IF NOT EXISTS certificates (
    id BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    full_name VARCHAR(120) NOT NULL,
    email VARCHAR(190) NOT NULL,
    certificate_code VARCHAR(40) NOT NULL,
    locations VARCHAR(255) NOT NULL DEFAULT 'harput_kalesi,urartu_sarnici_zindani',
    email_sent TINYINT(1) NOT NULL DEFAULT 0,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY uq_certificate_code (certificate_code),
    KEY idx_email (email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
