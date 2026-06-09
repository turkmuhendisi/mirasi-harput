#!/usr/bin/env bash
# AlmaLinux sunucuda İLK KURULUM
# Monorepo'dan yalnızca harput-app klasörünü indirir (unity-app, data vb. gelmez).
#
# Kullanım:
#   curl -fsSL .../setup-server.sh | bash
#   veya sunucuda: chmod +x setup-server.sh && ./setup-server.sh

set -euo pipefail

REPO_URL="${REPO_URL:-https://github.com/turkmuhendisi/mirasi-harput.git}"
BRANCH="${BRANCH:-main}"
INSTALL_DIR="${INSTALL_DIR:-/opt/mirasi-harput}"

echo "==> Miras'ı Harput sunucu kurulumu"
echo "    Repo: $REPO_URL"
echo "    Dizin: $INSTALL_DIR"
echo "    Yalnızca: harput-app/"
echo ""

if [ -d "$INSTALL_DIR/.git" ]; then
  echo "Git deposu zaten var: $INSTALL_DIR"
  echo "Güncelleme için: cd $INSTALL_DIR/harput-app/backend && ./deploy.sh"
  exit 0
fi

sudo mkdir -p "$(dirname "$INSTALL_DIR")"
sudo chown "$USER:$USER" "$(dirname "$INSTALL_DIR")" 2>/dev/null || true

echo "==> Sparse clone (sadece harput-app)..."
git clone --filter=blob:none --sparse --branch "$BRANCH" --depth 1 "$REPO_URL" "$INSTALL_DIR"

cd "$INSTALL_DIR"
git sparse-checkout set harput-app

echo ""
echo "==> İndirilen yapı:"
ls -la "$INSTALL_DIR/harput-app/"

echo ""
echo "==> Sonraki adımlar:"
echo "  1) cd $INSTALL_DIR/harput-app/backend"
echo "  2) cp .env.example .env && nano .env"
echo "  3) assets/fonts/ içine DejaVuSans.ttf yükleyin (opsiyonel)"
echo "  4) chmod +x deploy.sh && ./deploy.sh"
echo ""
echo "Detay: harput-app/backend/SUNUCU_KURULUM.md"
