import TextArgumentType = require('../base/Text');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { StringUtil } from '../../modules/util/StringUtil';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

const MIN_OPTIONS = 2;
const MAX_LENGTH = 2048;

class PollOptionsArgumentType extends TextArgumentType {
    get id(): string {
        return 'poll-options';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): string[] {
        const options = val.split(',').map(o => o.trim()).filter(o => o !== '');

        if (options.length < MIN_OPTIONS) {
            throw new ArgumentParsingError(
                `Can't start a poll with less than ${StringUtil.plural(MIN_OPTIONS, 'option')}, found ${StringUtil.plural(options.length, 'option')}.`
            );
        }

        const totalLength = options.reduce((acc, o) => acc + o.length + 20, 0);

        if (totalLength > MAX_LENGTH) {
            throw new ArgumentParsingError(`Can't start a poll with options that long, please reduce the number of options or their length.`);
        }

        return options;
    }
}

export = PollOptionsArgumentType;
