'use strict';

const DatabaseDriver = require('../database/DatabaseDriver.js');
const TaylorBotClient = require('./TaylorBotClient.js');
const Registry = require('./registry/Registry.js');
const Log = require('../tools/Logger.js');
const RedisDriver = require('../caching/RedisDriver.js');

class TaylorBotMasterClient {
    constructor() {
        this.database = new DatabaseDriver();
        this.redis = new RedisDriver();

        this.clients = [
            new TaylorBotClient(this)
        ];

        this.registry = new Registry(this.database, this.redis);
    }

    async load() {
        Log.info('Loading registry...');
        await this.registry.load();
        Log.info('Registry loaded!');

        return Promise.all(
            this.clients.map(c => c.load())
        );
    }

    start() {
        return Promise.all(
            this.clients.map(c => c.start())
        );
    }

    sendEmbed(textBasedChannel, embed) {
        return textBasedChannel.send('', { embed });
    }

    resolveUser(userId) {
        for (const client of this.clients) {
            const user = client.users.resolve(userId);

            if (user) {
                return user;
            }
        }

        return null;
    }
}

module.exports = TaylorBotMasterClient;