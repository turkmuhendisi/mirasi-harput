#!/bin/bash
set -e

STORAGE_DIR="/var/www/html/storage/certificates"

mkdir -p "$STORAGE_DIR"
chown -R www-data:www-data /var/www/html/storage
chmod -R 775 /var/www/html/storage

exec "$@"
