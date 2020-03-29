import { CommandRegistry } from './CommandRegistry';
import Command = require('../../commands/Command.js');
import { Guild } from 'discord.js';

export class CachedCommand {
    #commandRegistry: CommandRegistry;

    constructor(public name: string, public command: Command, commandRegistry: CommandRegistry) {
        this.#commandRegistry = commandRegistry;
    }

    async setEnabled(enabled: boolean): Promise<void> {
        await this.#commandRegistry.setGlobalEnabled(this.name, enabled);
    }

    async enableCommand(): Promise<void> {
        await this.setEnabled(true);
    }

    async disableCommand(): Promise<void> {
        await this.setEnabled(false);
    }

    async setGuildCommandEnabled(guild: Guild, enabled: boolean): Promise<void> {
        await this.#commandRegistry.setGuildEnabled(guild, this.name, enabled);
    }

    async enableIn(guild: Guild): Promise<void> {
        await this.setGuildCommandEnabled(guild, true);
    }

    async disableIn(guild: Guild): Promise<void> {
        await this.setGuildCommandEnabled(guild, false);
    }
}
