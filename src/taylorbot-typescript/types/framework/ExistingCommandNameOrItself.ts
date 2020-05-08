import { CommandMessageContext } from '../../commands/CommandMessageContext';
import TextArgumentType = require('../base/Text.js');
import ArgumentParsingError = require('../ArgumentParsingError.js');

class ExistingCommandNameOrItself extends TextArgumentType {
    get id(): string {
        return 'existing-command-name-or-itself';
    }

    canBeEmpty(): boolean {
        return true;
    }

    default({ command }: CommandMessageContext): string {
        return command.name;
    }

    async parse(val: string, { client }: CommandMessageContext): Promise<string> {
        const sanitizedInput = val.trim().toLowerCase();

        const result = await client.master.database.commands.getCommand(sanitizedInput);

        if (result != null)
            return result.name;

        const results = await client.master.database.commands.getCommandsFromModuleName(sanitizedInput);
        const notInService = results.find(({ name }) => client.master.registry.commands.resolve(name) === undefined);

        if (results.length === 0 || notInService === undefined)
            throw new ArgumentParsingError(`Command '${val}' doesn't exist.`);

        return notInService.name;
    }
}

export = ExistingCommandNameOrItself;
