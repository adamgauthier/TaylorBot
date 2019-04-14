#!/usr/bin/env bash
set -o errexit
set -o pipefail
set -o nounset

__dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

network_name=$(cat "${__dir}/../docker-network.name")

sed -i "/CREATE ROLE postgres;/d" ${__dir}/initdb/*.sql
sed -i -E "/ALTER ROLE postgres WITH SUPERUSER INHERIT CREATEROLE CREATEDB LOGIN REPLICATION BYPASSRLS PASSWORD '\w+';/d" ${__dir}/initdb/*.sql

image_name=taylorbot-postgres

docker build -t ${image_name} ${__dir}

curr_date=`date +%Y.%m.%d-%H.%M.%S`
container_path=/root/taylorbot-postgres/${curr_date}-container

data_path=${container_path}/pg-data
backups_path=${container_path}/pg-backups

mkdir -p ${data_path}
mkdir -p ${backups_path}

docker container run -d --name taylorbot-postgres --network ${network_name} --env-file ${__dir}/taylorbot-postgres.env --mount type=bind,source=${data_path},destination=/var/lib/postgresql/data --mount type=bind,source=${backups_path},destination=/home/pg-backups ${image_name}
