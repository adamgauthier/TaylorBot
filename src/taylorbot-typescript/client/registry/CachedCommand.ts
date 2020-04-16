import Command = require('../../commands/Command.js');

export class CachedCommand {
    constructor(public name: string, public command: Command) { }
}
