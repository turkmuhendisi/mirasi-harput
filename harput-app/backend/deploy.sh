#!/usr/bin/env bash
# Miras'ı Harput backend - sunucuda güncelleme ve yeniden başlatma
# Kullanım: ./deploy.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(git -C "$SCRIPT_DIR" rev-parse --show-toplevel 2>/dev/null || echo "$SCRIPT_DIR/..")"

echo "==> Git pull..."
cd "$REPO_ROOT"
git pull --ff-only

echo "==> Docker build & start (port 8087)..."
cd "$SCRIPT_DIR"
docker compose up -d --build

echo ""
echo "==> Durum:"
docker compose ps

echo ""
echo "==> Sağlık kontrolü:"
sleep 2
curl -fsS "http://127.0.0.1:8087/health.php" || echo "health.php henüz yanıt vermedi (ilk kurulumda .env kontrol edin)"

echo ""
echo "Hazır: http://187.127.83.10:8087/health.php"
