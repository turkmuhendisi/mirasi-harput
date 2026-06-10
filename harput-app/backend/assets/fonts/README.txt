DejaVu Sans fontları — Türkçe sertifika metinleri için.

Otomatik indirme (önerilen):
  cd backend
  ./scripts/download-fonts.sh

veya deploy sırasında otomatik:
  ./deploy.sh

Manuel: https://dejavu-fonts.github.io/ adresinden
  DejaVuSans.ttf
  DejaVuSans-Bold.ttf
dosyalarını bu klasöre koyun.

Docker: docker-compose.yml assets/ klasörünü volume ile bağlar;
fontlar sunucudaki backend/assets/fonts/ yolundan okunur.
