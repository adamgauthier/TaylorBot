import { URL } from 'url';

import TextArgumentType = require('../base/Text');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

class UrlArgumentType extends TextArgumentType {
    get id(): string {
        return 'url';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): URL {
        try {
            return new URL(val);
        }
        catch (e) {
            if (e instanceof TypeError) {
                throw new ArgumentParsingError(`Could not parse a valid URL from '${val}'`);
            }
            else {
                throw e;
            }
        }
    }
}

export = UrlArgumentType;
