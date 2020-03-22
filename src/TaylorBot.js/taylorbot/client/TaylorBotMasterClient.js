const DatabaseDriver = require('../database/DatabaseDriver.js');
const TaylorBotClient = require('./TaylorBotClient.js');
const Registry = require('./registry/Registry.js');
const Log = require('../tools/Logger.js');
const RedisDriver = require('../caching/RedisDriver.js');
const redisCommandsConfig = require('../config/redis-commands.json');
const redisHeistsConfig = require('../config/redis-heists.json');

class TaylorBotMasterClient {
    constructor() {
        this.database = new DatabaseDriver();
        this.redisCommands = new RedisDriver(redisCommandsConfig.HOST, redisCommandsConfig.PORT, redisCommandsConfig.PASSWORD);
        this.redisHeists = new RedisDriver(redisHeistsConfig.HOST, redisHeistsConfig.PORT, redisHeistsConfig.PASSWORD);

        this.clients = [
            new TaylorBotClient(this)
        ];

        this.registry = new Registry(this.database, this.redisCommands, this.redisHeists);
    }

    async load() {
        Log.info('Loading registry...');
        await this.registry.load();
        Log.info('Registry loaded!');

        return Promise.all(
            this.clients.map(c => c.load())
        );
    }

    unload() {
        Log.info('Unloading all clients...');
        this.clients.forEach(c => c.destroy());
        Log.info('Clients unloaded!');
    }

    start() {
        return Promise.all(
            this.clients.map(c => c.start())
        );
    }
}

module.exports = TaylorBotMasterClient;
