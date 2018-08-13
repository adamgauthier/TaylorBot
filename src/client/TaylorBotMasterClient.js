'use strict';

const { Paths } = require('globalobjects');

const DatabaseDriver = require(Paths.DatabaseDriver);
const TaylorBotClient = require('./TaylorBotClient.js');
const Registry = require(Paths.Registry);
const Log = require('../tools/Logger.js');

class TaylorBotMasterClient {
    constructor() {
        this.database = new DatabaseDriver();

        this.clients = [
            new TaylorBotClient(this)
        ];

        this.registry = new Registry(this.database);
    }

    async load() {
        Log.info('Loading database...');
        await this.database.load();
        Log.info('Database loaded!');

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