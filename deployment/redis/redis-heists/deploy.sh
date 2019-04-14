#!/usr/bin/env bash
set -o errexit
set -o pipefail
set -o nounset

__dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

network_name=$(cat "${__dir}/../../docker-network.name")
name=$(cat "${__dir}/redis-heists.name")
password=$(cat "${__dir}/redis-heists.pass")

docker container run -d --name $name --network ${network_name} redis redis-server --requirepass $password
