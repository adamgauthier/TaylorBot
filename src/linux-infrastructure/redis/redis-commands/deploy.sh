#!/usr/bin/env bash
set -o errexit
set -o pipefail
set -o nounset

__dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

network_name=$(cat "${__dir}/../../docker-network.name")
container_name=$(cat "${__dir}/redis-commands.name")
password=$(cat "${__dir}/redis-commands.pass")

curr_date=`date +%Y.%m.%d-%H.%M.%S`
container_path=~/${container_name}/${curr_date}-container

data_path=${container_path}/data

mkdir -p ${data_path}

docker container run \
    --detach \
    --name ${container_name} \
    --network ${network_name} \
    --mount type=bind,source=${data_path},destination=/data \
    redis redis-server --requirepass $password --save 3600 1 --loglevel warning
