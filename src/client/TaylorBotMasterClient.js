'use strict';

const { GlobalPaths } = require('globalobjects');

const DatabaseDriver = require(GlobalPaths.DatabaseDriver);
const TaylorBotClient = require(GlobalPaths.TaylorBotClient);
const Log = require(GlobalPaths.Logger);

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