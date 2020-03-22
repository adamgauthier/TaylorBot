#!/usr/bin/env bash
set -o errexit
set -o pipefail
set -o nounset

__dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

container_name=$(cat "${__dir}/taylorbot-node.name")
image_name=${container_name}
network_name=$(cat "${__dir}/../../docker-network.name")
taylorbot_node_path=${__dir}/../../../src/TaylorBot.js
build_context_path=${taylorbot_node_path}
docker_file_path=${taylorbot_node_path}/Dockerfile

docker build -t ${image_name} -f ${docker_file_path} ${build_context_path}
docker container run -d --name ${container_name} --network ${network_name} --restart=on-failure:100 ${image_name}
