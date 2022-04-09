#!/usr/bin/env bash
set -o errexit
set -o pipefail
set -o nounset

__dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

network_name=$(cat "${__dir}/../../../deploy/docker-network.name")
container_name=$(cat "${__dir}/taylorbot-postgres.name")

taylorbot_role_password=$(cat "${__dir}/taylorbot-role.pass")

sqitch_bundle_path=${__dir}/taylorbot-sqitch-bundle-extracted
[ -d ${sqitch_bundle_path} ] && rm -rf ${sqitch_bundle_path}
mkdir ${sqitch_bundle_path}
tar -xvf ${__dir}/taylorbot-sqitch-bundle.tgz --directory ${sqitch_bundle_path} --strip-components=1

homedst=/home

docker container run \
    --rm \
    --network ${network_name} \
    --mount type=bind,source=${sqitch_bundle_path},dst=/repo \
    --mount type=bind,src=${HOME},dst=${homedst} \
    --env HOME=${homedst} \
    sqitch/sqitch:latest \
    deploy db:pg://taylorbot:${taylorbot_role_password}@${container_name}/taylorbot
