import { URL } from 'url';
import UrlArgumentType = require('./Url');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

class HttpUrlArgumentType extends UrlArgumentType {
    get id(): string {
        return 'http-url';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): URL {
        const url = super.parse(val, commandContext, arg);

        if (!['http:', 'https:'].includes(url.protocol)) {
            throw new ArgumentParsingError(`URL '${val}' must be of http or https protocol.`);
        }

        return url;
    }
}

export = HttpUrlArgumentType;
