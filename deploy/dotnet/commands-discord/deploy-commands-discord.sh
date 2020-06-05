#!/usr/bin/env bash
set -o errexit
set -o pipefail
set -o nounset

__dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

network_name=$(cat "${__dir}/../../docker-network.name")
env_file_path=${__dir}/commands-discord.env
image_path=${__dir}/commands-discord.tar
container_name=taylorbot-commands-discord

load_ouput=$(docker load --input ${image_path})
image_name=${load_ouput##* }

docker container run \
    --detach \
    --mount source=${container_name},target=/app/Settings \
    --name ${container_name} \
    --network ${network_name} \
    --env-file ${env_file_path} \
    --restart=on-failure:100 \
    ${image_name}

docker logs --follow ${container_name}
