#!/usr/bin/env bash
set -o errexit
set -o pipefail
set -o nounset

__dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

network_name=$(cat "${__dir}/../../../deploy/docker-network.name")
container_name=$(cat "${__dir}/taylorbot-postgres.name")

curr_date=`date +%Y.%m.%d-%H.%M.%S`
container_path=~/${container_name}/${curr_date}-container

data_path=${container_path}/pg-data
backups_path=${container_path}/pg-backups

mkdir -p ${data_path}
mkdir -p ${backups_path}

docker container run \
    --detach \
    --name ${container_name} \
    --network ${network_name} \
    --env-file ${__dir}/taylorbot-postgres.env \
    --mount type=bind,source=${data_path},destination=/var/lib/postgresql/data \
    --mount type=bind,source=${backups_path},destination=/home/pg-backups \
    --publish 127.0.0.1:14487:5432/tcp \
    postgres:12

sleep 10s

taylorbot_role_password=$(cat "${__dir}/taylorbot-role.pass")

docker exec --interactive ${container_name} \
    psql --username=postgres --command="CREATE ROLE taylorbot WITH LOGIN PASSWORD '${taylorbot_role_password}';"
docker exec --interactive ${container_name} \
    createdb --username=postgres --owner=taylorbot taylorbot
docker exec --interactive ${container_name} \
    psql --username=postgres --dbname=taylorbot --command="CREATE EXTENSION IF NOT EXISTS pgcrypto WITH SCHEMA public;"
