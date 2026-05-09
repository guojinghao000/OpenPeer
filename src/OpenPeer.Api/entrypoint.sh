#!/bin/sh
set -e

echo "Waiting for PostgreSQL..."
until pg_isready -h openpeer-db -U openpeer; do
  sleep 2
done

echo "Applying database migrations..."
dotnet OpenPeer.Api.dll --migrate 2>/dev/null || true

echo "Starting API..."
exec dotnet OpenPeer.Api.dll
