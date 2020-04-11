import { CommandMessageContext } from '../../commands/CommandMessageContext';
import WordArgumentType = require('../base/Word.js');
import ArgumentParsingError = require('../ArgumentParsingError.js');

class ExistingCommandNameOrItself extends WordArgumentType {
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

        const result = await client.master.database.commands.getCommandName(sanitizedInput);

        if (result != null)
            return result.name;

        const results = await client.master.database.commands.getCommandNamesFromModuleName(sanitizedInput);
        const notInService = results.find(({ name }) => client.master.registry.commands.resolve(name) === undefined);

        if (results.length === 0 || notInService === undefined)
            throw new ArgumentParsingError(`Command '${val}' doesn't exist.`);

        return notInService.name;
    }
}

export = ExistingCommandNameOrItself;
