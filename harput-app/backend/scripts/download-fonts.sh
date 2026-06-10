#!/usr/bin/env bash
# DejaVu fontlarını backend/assets/fonts/ içine indirir (Türkçe sertifika metni için).
# Kullanım: ./scripts/download-fonts.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
FONTS_DIR="$(cd "$SCRIPT_DIR/.." && pwd)/assets/fonts"

REGULAR_URL="https://github.com/prawnpdf/prawn/raw/master/data/fonts/DejaVuSans.ttf"
BOLD_URL="https://github.com/prawnpdf/prawn/raw/master/data/fonts/DejaVuSans-Bold.ttf"

mkdir -p "$FONTS_DIR"

download() {
  local url="$1"
  local dest="$2"
  local name
  name="$(basename "$dest")"

  if [ -f "$dest" ] && [ -s "$dest" ]; then
    echo "  ✓ $name zaten var"
    return 0
  fi

  echo "  → $name indiriliyor..."
  if command -v curl >/dev/null 2>&1; then
    curl -fsSL "$url" -o "$dest"
  elif command -v wget >/dev/null 2>&1; then
    wget -q "$url" -O "$dest"
  else
    echo "HATA: curl veya wget gerekli"
    exit 1
  fi

  if [ ! -s "$dest" ]; then
    echo "HATA: $name indirilemedi"
    exit 1
  fi
  echo "  ✓ $name hazır"
}

echo "==> DejaVu fontları: $FONTS_DIR"
download "$REGULAR_URL" "$FONTS_DIR/DejaVuSans.ttf"
download "$BOLD_URL" "$FONTS_DIR/DejaVuSans-Bold.ttf"
echo "==> Tamam"
