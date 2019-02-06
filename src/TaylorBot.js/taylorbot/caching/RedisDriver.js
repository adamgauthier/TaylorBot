'use strict';

const { promisify } = require('util');
const redis = require('redis');

const redisConfig = require('../config/redis.json');

class RedisDriver {
    constructor() {
        this.redisClient = redis.createClient({
            host: redisConfig.HOST,
            port: redisConfig.PORT,
            password: redisConfig.PASSWORD
        });

        this.setAdd = promisify(this.redisClient.sadd).bind(this.redisClient);
        this.setRemove = promisify(this.redisClient.srem).bind(this.redisClient);
        this.setIsMember = promisify(this.redisClient.sismember).bind(this.redisClient);
    }
}

module.exports = RedisDriver;