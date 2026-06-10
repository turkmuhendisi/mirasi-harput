# Miras'ı Harput – Backend (PHP + Docker)

Sertifika üretimi ve e-posta gönderimi API'si. AlmaLinux sunucuda Docker ile çalışır.

**API adresi:** `http://187.127.83.10:8087/`

## Hızlı Kurulum (AlmaLinux sunucu)

### 1) Gereksinimler

```bash
# Docker + Compose (yoksa)
sudo dnf install -y docker docker-compose-plugin
sudo systemctl enable --now docker
sudo usermod -aG docker $USER
# Oturumu kapatıp açın veya: newgrp docker
```

### 2) Projeyi sunucuya alın (yalnızca harput-app)

Monorepo'da `unity-app`, `data` vb. var; sunucuda **sadece `harput-app`** gerekir:

```bash
cd /opt
git clone --filter=blob:none --sparse --branch main --depth 1 \
  https://github.com/turkmuhendisi/mirasi-harput.git mirasi-harput
cd mirasi-harput
git sparse-checkout set harput-app
cd harput-app/backend
```

Veya: `harput-app/setup-server.sh` scriptini çalıştırın.  
Detay: `SUNUCU_KURULUM.md`

### 3) Ortam dosyası

```bash
cp .env.example .env
nano .env   # DB, SMTP, API_KEY bilgilerini doldurun
```

**Font (Türkçe sertifika için):** `./scripts/download-fonts.sh` çalıştırın (veya `deploy.sh` bunu otomatik yapar). Manuel yükleme: `assets/fonts/DejaVuSans.ttf` ve `DejaVuSans-Bold.ttf`.

### 4) Veritabanı tablosu

Hostinger phpMyAdmin'de `sql/schema.sql` dosyasını bir kez çalıştırın.

### 5) İlk çalıştırma

```bash
chmod +x deploy.sh
./deploy.sh
```

Veya manuel:

```bash
docker compose up -d --build
```

### 6) Firewall (port 8087)

```bash
sudo firewall-cmd --permanent --add-port=8087/tcp
sudo firewall-cmd --reload
```

### 7) Test

```bash
curl http://127.0.0.1:8087/health.php
# Beklenen: {"success":true,"service":"mirasiharput-backend","database":"ok"}
```

Dışarıdan: `http://187.127.83.10:8087/health.php`

---

## Güncelleme (git pull)

GitHub'da değişiklik yaptıktan sonra sunucuda:

```bash
cd /opt/mirasi-harput/harput-app/backend
./deploy.sh
```

`deploy.sh` sırasıyla `git pull`, `docker compose up -d --build` yapar.

---

## API Uç Noktaları

| Method | URL | Açıklama |
|--------|-----|----------|
| GET | `/health.php` | Servis + DB kontrolü |
| POST | `/certificate.php` | Sertifika üret + e-posta gönder |

### POST örneği

```bash
curl -X POST http://187.127.83.10:8087/certificate.php \
  -H "Content-Type: application/json" \
  -d '{"fullName":"Ad Soyad","email":"test@example.com","apiKey":"SIZIN_API_KEY"}'
```

---

## Android bağlantısı

`app/src/main/kotlin/.../ApiConfig.kt`:

```kotlin
const val BASE_URL = "http://187.127.83.10:8087/"
const val API_KEY = "backend .env ile aynı"
```

---

## Sorun giderme

| Sorun | Çözüm |
|-------|--------|
| `database: error` | `.env` DB bilgileri; Hostinger Remote MySQL'de sunucu IP izinli mi? |
| Port erişilemiyor | `firewall-cmd` ile 8087 açık mı? `docker compose ps` çalışıyor mu? |
| E-posta gitmiyor | SMTP bilgileri `.env` içinde doğru mu? |
| Türkçe karakter bozuk | `assets/fonts/` altında TTF var mı? |

### Loglar

```bash
docker compose logs -f mirasiharput-api
```
