'use strict';

const { Paths } = require('globalobjects');

const DatabaseDriver = require(Paths.DatabaseDriver);
const TaylorBotClient = require(Paths.TaylorBotClient);
const Registry = require(Paths.Registry);
const Log = require(Paths.Logger);

class TaylorBotMasterClient {
    constructor() {
        this.database = new DatabaseDriver();

        this.clients = [
            new TaylorBotClient(this)
        ];

        this.oldRegistry = new Registry(this.database);
    }

    async load() {
        Log.info('Loading database...');
        await this.database.load();
        Log.info('Database loaded!');

        Log.info('Loading registry...');
        await this.oldRegistry.loadAll();
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
}

module.exports = TaylorBotMasterClient;