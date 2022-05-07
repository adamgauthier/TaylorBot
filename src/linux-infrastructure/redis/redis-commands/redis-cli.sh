#!/usr/bin/env bash
set -o errexit
set -o pipefail
set -o nounset

__dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

name=$(cat "${__dir}/redis-commands.name")
password=$(cat "${__dir}/redis-commands.pass")

docker exec -i -t $name redis-cli -a $password