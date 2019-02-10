#!/usr/bin/env bash
sed -i "/CREATE ROLE postgres;/d" ./initdb/*.sql
sed -i -E "/ALTER ROLE postgres WITH SUPERUSER INHERIT CREATEROLE CREATEDB LOGIN REPLICATION BYPASSRLS PASSWORD '\w+';/d" ./initdb/*.sql

docker build -t taylorbot-postgres .

curr_date=`date +%Y.%m.%d-%H.%M.%S`
container_path=/root/taylorbot-postgres/${curr_date}-container

data_path=${container_path}/pg-data
backups_path=${container_path}/pg-backups

mkdir -p ${data_path}
mkdir -p ${backups_path}

docker container run -d --name taylorbot-postgres --env-file taylorbot-postgres.env -p 127.0.0.1:5432:5432 --mount type=bind,source=${data_path},destination=/var/lib/postgresql/data --mount type=bind,source=${backups_path},destination=/home/pg-backups taylorbot-postgres
