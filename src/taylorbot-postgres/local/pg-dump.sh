#!/usr/bin/env bash
set -o errexit
set -o pipefail
set -o nounset

__dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

container_name=$(cat "${__dir}/taylorbot-postgres.name")

docker exec --interactive ${container_name} \
    pg_dump --username postgres --dbname taylorbot \
    --file /home/pg-backups/`date +%Y.%m.%d-%H.%M.%S`_dump.sql
