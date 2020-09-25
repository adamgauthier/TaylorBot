import WordArgumentType = require('../base/Word');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

class SubredditArgumentType extends WordArgumentType {
    get id(): string {
        return 'subreddit';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): string {
        const subreddit = val.trim();
        const matches = subreddit.match(/^(?:\/?r\/)?([A-Za-z0-9][A-Za-z0-9_]{2,20})$/);
        if (matches) {
            return matches[1];
        }

        throw new ArgumentParsingError(`Could not parse '${val}' into a valid subreddit name.`);
    }
}

export = SubredditArgumentType;
