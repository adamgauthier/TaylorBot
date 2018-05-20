'use strict';

const { Paths } = require('globalobjects');

const DatabaseDriver = require(Paths.DatabaseDriver);
const TaylorBotClient = require(Paths.TaylorBotClient);
const Log = require(Paths.Logger);

class TaylorBotMasterClient {
    constructor() {
        this.database = new DatabaseDriver();

        this.clients = [
            new TaylorBotClient(this)
        ];
    }

    async start() {
        Log.info('Loading database...');
        await this.database.load();
        Log.info('Database loaded!');

        return Promise.all(
            this.clients.map(c => c.start())
        );
    }
}

module.exports = TaylorBotMasterClient;