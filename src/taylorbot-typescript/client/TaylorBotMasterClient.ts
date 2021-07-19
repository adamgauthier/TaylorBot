import redisCommandsConfig = require('../config/redis-commands.json');

import { DatabaseDriver } from '../database/DatabaseDriver';
import { TaylorBotClient } from './TaylorBotClient';
import { Registry } from './registry/Registry';
import { Log } from '../tools/Logger';
import { RedisDriver } from '../caching/RedisDriver';
import { EnvUtil } from '../modules/util/EnvUtil';

const redisCommandsPassword = EnvUtil.getRequiredEnvVariable('TaylorBot_RedisCommandsConnection__Password');

export class TaylorBotMasterClient {
    readonly #client: TaylorBotClient;

    readonly database = new DatabaseDriver();
    readonly registry: Registry;

    constructor() {
        const redis = new RedisDriver(redisCommandsConfig.HOST, redisCommandsConfig.PORT, redisCommandsPassword);

        this.#client = new TaylorBotClient(this);

        this.registry = new Registry(
            this.database,
            redis
        );
    }

    async load(): Promise<void> {
        Log.info('Loading registry...');
        await this.registry.load();
        Log.info('Registry loaded!');

        await this.#client.load();
    }

    unload(): void {
        Log.info('Unloading client...');
        this.#client.destroy();
        Log.info('Client unloaded!');
    }

    async start(): Promise<void> {
        await this.#client.start();
    }
}
