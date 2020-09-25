import WordArgumentType = require('../base/Word');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

const usernameRegex = '([a-z0-9_](?:(?:[a-z0-9_]|(?:\\.(?!\\.))){0,28}(?:[a-z0-9_]))?)';

class InstagramUsernameArgumentType extends WordArgumentType {
    get id(): string {
        return 'instagram-username';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): string {
        const instagram = val.trim();
        const matches = instagram.match(new RegExp(`^${usernameRegex}$`, 'i'));
        if (matches) {
            return matches[1];
        }
        else {
            try {
                const url = new URL(instagram);
                if (url.hostname === 'www.instagram.com') {
                    const matches = url.pathname.match(new RegExp(`^\\/${usernameRegex}(\\/.*)?$`, 'i'));
                    if (matches)
                        return matches[1];
                }
            }
            catch (e) {
                // Continue on error, it's not a URL
            }
        }

        throw new ArgumentParsingError(`Could not parse '${val}' into a valid Instagram username.`);
    }
}

export = InstagramUsernameArgumentType;
