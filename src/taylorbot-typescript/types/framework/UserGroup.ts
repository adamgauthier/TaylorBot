import WordArgumentType = require('../base/Word');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

class UserGroupArgumentType extends WordArgumentType {
    get id(): string {
        return 'user-group';
    }

    parse(val: string, { client }: CommandMessageContext, arg: CommandArgumentInfo): { name: string; accessLevel: number } {
        for (const group of client.master.registry.groups.values()) {
            if (group.name.toLowerCase() === val.toLowerCase())
                return group;
        }

        throw new ArgumentParsingError(`User Group '${val.toLowerCase()}' doesn't exist.`);
    }
}

export = UserGroupArgumentType;
