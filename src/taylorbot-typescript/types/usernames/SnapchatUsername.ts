import WordArgumentType = require('../base/Word');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

const usernameRegex = '([a-z][a-z0-9\\-_\\.]{1,13}[a-z0-9])';

class SnapchatUsernameArgumentType extends WordArgumentType {
    get id(): string {
        return 'snapchat-username';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): string {
        const snapchat = val.trim();
        const matches = snapchat.match(new RegExp(`^${usernameRegex}$`, 'i'));
        if (matches) {
            return matches[1];
        }
        else {
            try {
                const url = new URL(snapchat);
                if (url.hostname === 'www.snapchat.com') {
                    const matches = url.pathname.match(new RegExp(`^\\/add\\/${usernameRegex}(\\/.*)?$`, 'i'));
                    if (matches)
                        return matches[1];
                }
            }
            catch (e) {
                // Continue on error, it's not a URL
            }
        }

        throw new ArgumentParsingError(`Could not parse '${val}' into a valid Snapchat username.`);
    }
}

export = SnapchatUsernameArgumentType;
