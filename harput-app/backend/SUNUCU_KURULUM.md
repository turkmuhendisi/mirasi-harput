# Sunucuda Oluşturmanız Gerekenler (AlmaLinux – 187.127.83.10)

Bu dosyalar **GitHub'a gitmez**. Sunucuda elle oluşturulur veya kopyalanır.

---

## 1) `backend/.env` (zorunlu)

```bash
cd harput-app/backend
cp .env.example .env
nano .env
```

Doldurulması gereken alanlar:

| Alan | Açıklama | Örnek |
|------|----------|-------|
| `DB_HOST` | Hostinger MySQL sunucusu | `92.113.22.53` |
| `DB_PORT` | MySQL portu | `3306` |
| `DB_NAME` | Veritabanı adı | `u824624299_harput` |
| `DB_USER` | DB kullanıcı adı | *(Hostinger panelinden)* |
| `DB_PASSWORD` | DB şifresi | *(Hostinger panelinden)* |
| `MAIL_HOST` | SMTP sunucusu | `smtp.hostinger.com` |
| `MAIL_PORT` | SMTP portu | `465` |
| `MAIL_ENCRYPTION` | `ssl` veya `tls` | `ssl` |
| `MAIL_USERNAME` | E-posta hesabı | `info@sizin-domain.com` |
| `MAIL_PASSWORD` | E-posta şifresi | *(mail hesabı şifresi)* |
| `MAIL_FROM_ADDRESS` | Gönderen adres | `info@sizin-domain.com` |
| `MAIL_FROM_NAME` | Gönderen adı | `Miras'ı Harput` |
| `API_KEY` | API güvenlik anahtarı | *(güçlü rastgele bir string)* |

Opsiyonel (varsayılanlar genelde yeterli):

- `CERT_FONT_REGULAR=assets/fonts/DejaVuSans.ttf`
- `CERT_FONT_BOLD=assets/fonts/DejaVuSans-Bold.ttf`
- `CERT_TEMPLATE=assets/certificate-template.png`

---

## 2) Font dosyaları (önerilir – Türkçe karakterler için)

Sunucuda:

```bash
mkdir -p harput-app/backend/assets/fonts
```

Bu klasöre yükleyin (FTP/SCP ile):

- `DejaVuSans.ttf`
- `DejaVuSans-Bold.ttf`

İndirme: https://dejavu-fonts.github.io/

---

## 3) Veritabanı tablosu (bir kez – Hostinger phpMyAdmin)

`sql/schema.sql` içeriğini `u824624299_harput` veritabanında çalıştırın.

---

## 4) Hostinger Remote MySQL izni (bir kez)

Hostinger panel → Remote MySQL → şu IP'yi ekleyin:

```text
187.127.83.10
```

(Böylece Docker backend Hostinger DB'ye bağlanabilir.)

---

## 5) Firewall kuralı (bir kez)

```bash
sudo firewall-cmd --permanent --add-port=8087/tcp
sudo firewall-cmd --reload
```

---

## 6) Android `ApiConfig.kt` (geliştirme makinenizde)

Git'e **gerçek API anahtarını yazmayın**; localde düzenleyin:

`app/src/main/kotlin/com/mirasiharput/features/certificate/ApiConfig.kt`

```kotlin
const val BASE_URL = "http://187.127.83.10:8087/"
const val API_KEY = "backend .env ile AYNI değer"
```

> `API_KEY` boş bırakılırsa backend'de de `API_KEY` boş olmalı (kontrol devre dışı).

---

## 7) Android `local.properties` (sadece kendi bilgisayarınızda)

Android Studio otomatik oluşturur; **commit etmeyin**.

```properties
sdk.dir=/Users/.../Library/Android/sdk
```

---

## Git'e GİTMEYEN dosya özeti

| Dosya / klasör | Nerede |
|----------------|--------|
| `.env` | `backend/` |
| `vendor/` | `backend/` (composer install ile oluşur) |
| `storage/certificates/*.png` | `backend/` |
| `local.properties` | `harput-app/` |
| `secrets.properties` | `harput-app/` (opsiyonel) |
| `*.keystore` / `*.jks` | imzalama anahtarları |

---

## İlk çalıştırma

```bash
cd harput-app/backend
chmod +x deploy.sh
./deploy.sh
```

Test:

```bash
curl http://187.127.83.10:8087/health.php
```

Beklenen: `"database":"ok"`

---

## Güncelleme

```bash
cd harput-app/backend
./deploy.sh
```

`.env` dosyanız korunur; sadece kod imajı yenilenir.
