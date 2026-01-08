#!/usr/bin/env bash
set -euo pipefail

# Simple backup loop (dev-friendly). Stores .bak files under /backups.
# Schedule is controlled by BACKUP_INTERVAL_SECONDS (default: 86400 = 24h)

interval="${BACKUP_INTERVAL_SECONDS:-86400}"
server="${MSSQL_SERVER:-sqlserver}"
port="${MSSQL_PORT:-1433}"
database="${MSSQL_DATABASE:-CoinDB}"
user="${MSSQL_USER:-sa}"
password="${MSSQL_PASSWORD:?MSSQL_PASSWORD is required}"
backup_dir="${BACKUP_DIR:-/backups}"

mkdir -p "$backup_dir"

while true; do
  ts="$(date -u +%Y%m%dT%H%M%SZ)"
  file="$backup_dir/${database}_${ts}.bak"

  echo "[backup] Creating backup: $file"

  /opt/mssql-tools/bin/sqlcmd -S "${server},${port}" -U "$user" -P "$password" \
    -Q "BACKUP DATABASE [${database}] TO DISK='${file}' WITH INIT, COMPRESSION"

  echo "[backup] Done. Sleeping ${interval}s"
  sleep "$interval"
done
