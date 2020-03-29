import Log = require('../../tools/Logger.js');
import { CachedCommand } from './CachedCommand.js';
import { CommandLoader } from '../../commands/CommandLoader';
import AttributeLoader = require('../../attributes/AttributeLoader.js');
import DatabaseDriver = require('../../database/DatabaseDriver.js');
import RedisDriver = require('../../caching/RedisDriver.js');
import Command = require('../../commands/Command.js');
import { Guild } from 'discord.js';

export class CommandRegistry {
    database: DatabaseDriver;
    redis: RedisDriver;
    #commandsCache = new Map<string, CachedCommand>();
    useCountCache = new Map<string, { count: number; errorCount: number }>();

    constructor(database: DatabaseDriver, redis: RedisDriver) {
        this.database = database;
        this.redis = redis;
    }

    async loadAll(): Promise<void> {
        const databaseCommands: { name: string }[] = await this.database.commands.getAll();

        const commands = [
            ...(await CommandLoader.loadAll()),
            ...(await AttributeLoader.loadMemberAttributeCommands()),
            ...(await AttributeLoader.loadUserAttributeCommands())
        ];

        const fileCommandsNotInDatabase = commands.filter(command =>
            !databaseCommands.some(c => c.name === command.name)
        );

        if (fileCommandsNotInDatabase.length > 0) {
            Log.info(`Found new file commands ${fileCommandsNotInDatabase.map(c => c.name).join(',')}. Adding to database.`);

            const inserted = await this.database.commands.addAll(
                fileCommandsNotInDatabase.map(command => {
                    return { 'name': command.name };
                })
            );

            databaseCommands.push(...inserted);
        }

        commands.forEach(c => this.cacheCommand(c));
    }

    cacheCommand(command: Command): void {
        const key = command.name.toLowerCase();

        if (this.#commandsCache.has(key))
            throw new Error(`Command '${command.name}' is already cached.`);

        const cached = new CachedCommand(
            command.name,
            command,
            this
        );

        this.#commandsCache.set(key, cached);

        for (const alias of command.aliases) {
            const aliasKey = alias.toLowerCase();

            if (this.#commandsCache.has(aliasKey))
                throw new Error(`Command Key '${aliasKey}' is already cached when setting alias.`);

            this.#commandsCache.set(aliasKey, key);
        }
    }

    getCommand(name: string): CachedCommand {
        const cachedCommand = this.#commandsCache.get(name);

        if (!cachedCommand)
            throw new Error(`Command '${name}' isn't cached.`);

        if (typeof (cachedCommand) === 'string')
            throw new Error(`Command '${name}' is cached as an alias.`);

        return cachedCommand;
    }

    resolve(commandName: string): CachedCommand | undefined {
        const command = this.#commandsCache.get(commandName.toLowerCase());

        if (typeof (command) === 'string') {
            return this.getCommand(command);
        }

        return command;
    }

    addSuccessfulUseCount(command: CachedCommand): void {
        const useCount = this.useCountCache.get(command.name);
        if (!useCount) {
            this.useCountCache.set(command.name, { count: 1, errorCount: 0 });
        }
        else {
            useCount.count += 1;
        }
    }

    addUnhandledErrorCount(command: CachedCommand): void {
        const useCount = this.useCountCache.get(command.name);
        if (!useCount) {
            this.useCountCache.set(command.name, { count: 0, errorCount: 1 });
        }
        else {
            useCount.errorCount += 1;
        }
    }

    getAllCommands(): CachedCommand[] {
        return Array.from(
            this.#commandsCache.values()
        ).filter(val => typeof (val) !== 'string');
    }

    get enabledRedisKey(): string {
        return 'enabled-commands';
    }

    async insertOrGetIsCommandDisabled(command: CachedCommand): Promise<boolean> {
        const isEnabled = await this.redis.hashGet(this.enabledRedisKey, command.name);

        if (isEnabled === null) {
            const { enabled } = await this.database.commands.insertOrGetIsCommandDisabled(command);
            await this.redis.hashSet(this.enabledRedisKey, command.name, enabled ? 1 : 0);
            return !enabled;
        }

        return isEnabled === '0';
    }

    async setGlobalEnabled(commandName: string, setEnabled: boolean): Promise<boolean> {
        const { enabled } = await this.database.commands.setEnabled(commandName, setEnabled);
        await this.redis.hashSet(this.enabledRedisKey, commandName, enabled);
        return enabled;
    }

    enabledGuildRedisKey(guild: Guild): string {
        return `enabled-commands:guild:${guild.id}`;
    }

    async getIsGuildCommandDisabled(guild: Guild, command: CachedCommand): Promise<boolean> {
        const isEnabled = await this.redis.hashGet(this.enabledGuildRedisKey(guild), command.name);

        if (isEnabled === null) {
            const { exists } = await this.database.guildCommands.getIsGuildCommandDisabled(guild, command);
            await this.redis.hashSet(this.enabledGuildRedisKey(guild), command.name, (!exists) ? 1 : 0);
            await this.redis.expire(this.enabledGuildRedisKey(guild), 6 * 60 * 60);
            return exists;
        }

        return isEnabled === '0';
    }

    async setGuildEnabled(guild: Guild, commandName: string, enabled: boolean): Promise<boolean> {
        const { disabled } = await this.database.guildCommands.setDisabled(guild, commandName, !enabled);
        await this.redis.hashSet(this.enabledGuildRedisKey(guild), commandName, (!disabled) ? 1 : 0);
        return !disabled;
    }
}
