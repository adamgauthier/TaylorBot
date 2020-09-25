import WordArgumentType = require('../base/Word');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

class ChannelLogTypeArgumentType extends WordArgumentType {
    get id(): string {
        return 'channel-log-type';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): 'member' | 'message' {
        const word = super.parse(val, commandContext, arg);
        const clean = word.trim().toLowerCase();

        switch (clean) {
            case 'member':
                return 'member';
            case 'message':
                return 'message';
            default:
                throw new ArgumentParsingError(`Log type must be 'member' or 'message'.`);
        }
    }
}

export = ChannelLogTypeArgumentType;
