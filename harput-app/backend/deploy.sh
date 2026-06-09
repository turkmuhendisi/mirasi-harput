#!/usr/bin/env bash
# Backend güncelleme: git pull (yalnızca harput-app değişiklikleri) + docker rebuild
# Sunucuda: cd /opt/mirasi-harput/harput-app/backend && ./deploy.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
HARPUT_APP_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

# Git kökü: sparse clone'da /opt/mirasi-harput, tam clone'da aynı
GIT_ROOT="$(git -C "$SCRIPT_DIR" rev-parse --show-toplevel 2>/dev/null || true)"

echo "==> Miras'ı Harput backend deploy"
echo "    Backend: $SCRIPT_DIR"

if [ -n "$GIT_ROOT" ] && [ -d "$GIT_ROOT/.git" ]; then
  echo "==> Git pull ($GIT_ROOT)..."
  cd "$GIT_ROOT"
  git pull --ff-only origin "${BRANCH:-main}" 2>/dev/null || git pull --ff-only
  echo "    (Monorepo'dan yalnızca harput-app kullanılıyor; unity-app indirilmez/güncellenmez sparse checkout ile)"
else
  echo "==> Git deposu yok, pull atlanıyor (manuel kopya?)"
fi

echo "==> Docker build & start (port 8087)..."
cd "$SCRIPT_DIR"

if [ ! -f .env ]; then
  echo "HATA: .env bulunamadı. Önce: cp .env.example .env && nano .env"
  exit 1
fi

docker compose up -d --build

echo ""
echo "==> Durum:"
docker compose ps

echo ""
echo "==> Sağlık kontrolü:"
sleep 2
curl -fsS "http://127.0.0.1:8087/health.php" || echo "health.php yanıt vermedi (.env / DB kontrol edin)"

echo ""
echo "Hazır: http://187.127.83.10:8087/health.php"
