import { Command } from '../../commands/Command';

export class CachedCommand {
    constructor(public name: string, public command: Command) { }
}
