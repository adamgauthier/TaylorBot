import { CommandMessageContext } from '../../commands/CommandMessageContext';
import TextArgumentType = require('../base/Text');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { DatabaseCommand } from '../../database/repositories/CommandRepository';

class DatabaseCommandArgumentType extends TextArgumentType {
    get id(): string {
        return 'database-command';
    }

    async parse(val: string, { client }: CommandMessageContext): Promise<DatabaseCommand> {
        const sanitizedInput = val.trim().toLowerCase();

        const result = await client.master.database.commands.getCommand(sanitizedInput);

        if (result != null)
            return result;

        throw new ArgumentParsingError(`Command '${val}' doesn't exist.`);
    }
}

export = DatabaseCommandArgumentType;
