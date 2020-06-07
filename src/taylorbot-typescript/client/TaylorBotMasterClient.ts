import redisCommandsConfig = require('../config/redis-commands.json');
import redisHeistsConfig = require('../config/redis-heists.json');

import { DatabaseDriver } from '../database/DatabaseDriver';
import { TaylorBotClient } from './TaylorBotClient';
import { Registry } from './registry/Registry';
import Log = require('../tools/Logger.js');
import { RedisDriver} from '../caching/RedisDriver';

export class TaylorBotMasterClient {
    #clients: TaylorBotClient[];

    database = new DatabaseDriver();
    registry: Registry;

    constructor() {
        const redisCommands = new RedisDriver(redisCommandsConfig.HOST, redisCommandsConfig.PORT, redisCommandsConfig.PASSWORD);
        const redisHeists = new RedisDriver(redisHeistsConfig.HOST, redisHeistsConfig.PORT, redisHeistsConfig.PASSWORD);

        this.#clients = [
            new TaylorBotClient(this)
        ];

        this.registry = new Registry(
            this.database,
            redisCommands,
            redisHeists
        );
    }

    async load(): Promise<void[]> {
        Log.info('Loading registry...');
        await this.registry.load();
        Log.info('Registry loaded!');

        return Promise.all(
            this.#clients.map(c => c.load())
        );
    }

    unload(): void {
        Log.info('Unloading all clients...');
        this.#clients.forEach(c => c.destroy());
        Log.info('Clients unloaded!');
    }

    start(): Promise<void[]> {
        return Promise.all(
            this.#clients.map(c => c.start())
        );
    }
}
