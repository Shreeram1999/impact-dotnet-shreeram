#!/bin/bash
# One-shot container: once SQL Server is healthy, create the databases that
# are NOT owned by EF migrations and run their scripts.
#   StudentPortal_Identity  <- Identity's own ADO.NET schema + stored procedures
#   StudentPortal_Reporting <- the data team's "pre-existing" reporting schema
# (Academics creates and migrates StudentPortal_Academics itself on start.)
# Every script is idempotent, so `docker compose up` can run this every time.
set -euo pipefail

SQLCMD=/opt/mssql-tools18/bin/sqlcmd
run() { "$SQLCMD" -S sqlserver -U sa -P "$SA_PASSWORD" -C -b "$@"; }

run -Q "IF DB_ID(N'StudentPortal_Identity') IS NULL CREATE DATABASE StudentPortal_Identity;
        IF DB_ID(N'StudentPortal_Reporting') IS NULL CREATE DATABASE StudentPortal_Reporting;"

for script in /scripts/identity/*.sql; do
  echo "StudentPortal_Identity  <- $(basename "$script")"
  run -d StudentPortal_Identity -i "$script"
done

for script in /scripts/reporting/*.sql; do
  echo "StudentPortal_Reporting <- $(basename "$script")"
  run -d StudentPortal_Reporting -i "$script"
done

echo "db-init complete."
