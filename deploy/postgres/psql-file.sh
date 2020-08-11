#!/usr/bin/env bash
set -o errexit
set -o pipefail
set -o nounset

if [ $# -lt 1 ]; then
    echo "You must use specify a file name"
    exit 1
fi

__dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

container_name=$(cat "${__dir}/taylorbot-postgres.name")

docker exec --interactive ${container_name} \
    psql --username=postgres --dbname=taylorbot --file=/home/pg-backups/$1
