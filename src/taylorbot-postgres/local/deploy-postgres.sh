#!/usr/bin/env bash
set -o errexit
set -o pipefail
set -o nounset

__dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

network_name=$(cat "${__dir}/../../linux-infrastructure/docker-network.name")
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
    --publish 14487:5432 \
    postgres:15

echo "Waiting for database server to be ready..."
sleep 20

taylorbot_role_password=$(cat "${__dir}/taylorbot-role.pass")

echo "Creating taylorbot database..."
docker exec --interactive ${container_name} \
    psql --username=postgres --command="CREATE ROLE taylorbot WITH LOGIN PASSWORD '${taylorbot_role_password}';"
docker exec --interactive ${container_name} \
    psql --username=postgres --command="CREATE DATABASE taylorbot;"
docker exec --interactive ${container_name} \
    psql --username=postgres --command="GRANT ALL PRIVILEGES ON DATABASE taylorbot TO taylorbot;"
docker exec --interactive ${container_name} \
    psql --username=postgres --dbname=taylorbot --command="GRANT ALL ON SCHEMA public TO taylorbot;"
