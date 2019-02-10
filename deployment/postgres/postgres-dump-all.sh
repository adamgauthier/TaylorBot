#!/usr/bin/env bash
docker exec -t taylorbot-postgres pg_dumpall -U postgres -f /home/pg-backups/`date +%Y.%m.%d-%H.%M.%S`_cluster_dumpall.sql
