#!/usr/bin/env bash
set -o errexit
set -o pipefail
set -o nounset

__dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

name=$(cat "${__dir}/redis-heists.name")
password=$(cat "${__dir}/redis-heists.pass")

docker exec -i -t $name redis-cli -a $password