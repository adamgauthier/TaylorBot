#!/usr/bin/env bash
set -o errexit
set -o pipefail
set -o nounset

__dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

container_name=$(cat "${__dir}/taylorbot-postgres.name")

docker exec -i -t ${container_name} psql -U postgres taylorbot
