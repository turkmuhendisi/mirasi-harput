# Sunucu Kurulumu (AlmaLinux – 187.127.83.10)

Repoda `unity-app`, `data`, `docs` vb. de var. **Sunucuda yalnızca `harput-app` kullanılır.**

---

## İlk kurulum (önerilen – sparse clone)

Sunucuda sadece `harput-app` klasörü indirilir; diğer klasörler gelmez.

```bash
# İlk kurulum scripti (harput-app içinde)
export INSTALL_DIR=/opt/mirasi-harput   # isteğe bağlı, varsayılan bu
curl -fsSL https://raw.githubusercontent.com/turkmuhendisi/mirasi-harput/main/harput-app/setup-server.sh | bash
```

Veya elle:

```bash
sudo mkdir -p /opt && cd /opt
git clone --filter=blob:none --sparse --branch main --depth 1 \
  https://github.com/turkmuhendisi/mirasi-harput.git mirasi-harput
cd mirasi-harput
git sparse-checkout set harput-app
```

Sunucudaki dizin yapısı:

```text
/opt/mirasi-harput/
  harput-app/          ← sadece bu kullanılır
    app/               (Android kaynak – sunucuda çalıştırılmaz)
    backend/           ← Docker API burada
      .env             (siz oluşturursunuz, git'e gitmez)
      deploy.sh
      docker-compose.yml
```

---

## Sunucuda oluşturmanız gerekenler

### 1) `backend/.env` (zorunlu)

```bash
cd /opt/mirasi-harput/harput-app/backend
cp .env.example .env
nano .env
```

| Alan | Açıklama |
|------|----------|
| `DB_USER`, `DB_PASSWORD` | Hostinger MySQL |
| `MAIL_USERNAME`, `MAIL_PASSWORD`, `MAIL_FROM_ADDRESS` | SMTP |
| `API_KEY` | Güçlü rastgele anahtar (Android `ApiConfig` ile aynı) |

### 2) Font dosyaları (önerilir)

```bash
cd /opt/mirasi-harput/harput-app/backend
./scripts/download-fonts.sh
# deploy.sh çalıştırırsanız fontlar yoksa otomatik indirilir
```

### 3) Hostinger (bir kez)

- Remote MySQL'e `187.127.83.10` IP izni verin
- `certificates` tablosu ilk API isteğinde otomatik oluşturulur (DB kullanıcısının `CREATE` yetkisi olmalı)
- Otomatik oluşmazsa phpMyAdmin'de `sql/schema.sql` çalıştırın

### 4) Firewall (bir kez)

```bash
sudo firewall-cmd --permanent --add-port=8087/tcp
sudo firewall-cmd --reload
```

### 5) Docker (bir kez)

```bash
sudo dnf install -y docker docker-compose-plugin
sudo systemctl enable --now docker
sudo usermod -aG docker $USER
# Oturumu yenileyin
```

---

## Çalıştırma

```bash
cd /opt/mirasi-harput/harput-app/backend
chmod +x deploy.sh
./deploy.sh
```

Test:

```bash
curl http://187.127.83.10:8087/health.php
```

---

## Güncelleme (GitHub'dan)

Kod değiştirdikten sonra sunucuda:

```bash
cd /opt/mirasi-harput/harput-app/backend
./deploy.sh
```

`deploy.sh` → `git pull` + `docker compose up -d --build`  
`.env` dosyanız korunur.

---

## Git'e GİTMEYEN dosyalar

| Dosya | Konum |
|-------|--------|
| `.env` | `backend/` |
| `vendor/` | `backend/` |
| `storage/certificates/*.png` | `backend/` |

---

## Android (kendi bilgisayarınız)

`ApiConfig.kt`:

```kotlin
const val BASE_URL = "http://187.127.83.10:8087/"
const val API_KEY = "backend .env ile aynı"
```

`local.properties` commit etmeyin.
