#!/usr/bin/env bash
set -o errexit
set -o pipefail
set -o nounset

docker exec -i -t taylorbot-redis redis-cli