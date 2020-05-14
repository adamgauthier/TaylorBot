#!/usr/bin/env bash
set -o errexit
set -o pipefail
set -o nounset

__dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

container_name=$(cat "${__dir}/stats-tracker.name")
image_name=${container_name}
network_name=$(cat "${__dir}/../../docker-network.name")
env_file_path=${__dir}/../taylorbot.net.env
taylorbot_net_path=${__dir}/../../../src/TaylorBot.Net
build_context_path=${taylorbot_net_path}
docker_file_path=${taylorbot_net_path}/Program.StatsTracker/src/TaylorBot.Net.StatsTracker.Program/Dockerfile

docker build -t ${image_name} -f ${docker_file_path} ${build_context_path}
docker container run -d --name ${container_name} --network ${network_name} --env-file ${env_file_path} --restart=on-failure:100 ${image_name}
