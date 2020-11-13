import { URL } from 'url';
import WordArgumentType = require('../base/Word');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

const usernameRegex = '([a-z0-9](?:[a-z0-9-]{1,30}[a-z0-9])?)';

class TumblrUsernameArgumentType extends WordArgumentType {
    get id(): string {
        return 'tumblr-username';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): string {
        const tumblr = val.trim();
        const matches = tumblr.match(new RegExp(`^${usernameRegex}$`, 'i'));
        if (matches) {
            return matches[1];
        }
        else {
            try {
                const url = new URL(tumblr);
                const matches = url.hostname.match(new RegExp(`^${usernameRegex}.tumblr.com(\\/.*)?$`, 'i'));
                if (matches)
                    return matches[1];
            }
            catch (e) {
                // Continue on error, it's not a URL
            }
        }

        throw new ArgumentParsingError(`Could not parse '${val}' into a valid Tumblr username.`);
    }
}

export = TumblrUsernameArgumentType;
