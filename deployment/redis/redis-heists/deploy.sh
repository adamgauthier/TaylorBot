#!/usr/bin/env bash
set -o errexit
set -o pipefail
set -o nounset

__dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

name=$(cat "${__dir}/redis-heists.name")
port=$(cat "${__dir}/redis-heists.port")
password=$(cat "${__dir}/redis-heists.pass")

docker container run -d --name $name -p 127.0.0.1:$port:6379 redis redis-server --requirepass $password