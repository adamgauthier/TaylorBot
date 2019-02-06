#!/usr/bin/env bash
set -o errexit
set -o pipefail
set -o nounset

__dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

password=$(cat "${__dir}/redis.pass")

docker container run -d --name taylorbot-redis -p 127.0.0.1:6379:6379 redis redis-server --requirepass $password