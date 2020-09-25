import moment = require('moment');
import Sherlock = require('sherlockjs');
import { SherlockResult } from 'sherlockjs';

import TextArgumentType = require('../base/Text');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

class FutureEventArgumentType extends TextArgumentType {
    get id(): string {
        return 'future-event';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): SherlockResult {
        const sherlocked = Sherlock.parse(val);

        if (sherlocked.startDate !== null && sherlocked.eventTitle !== '') {
            if (moment(sherlocked.startDate).isAfter()) {
                return sherlocked;
            }
            throw new ArgumentParsingError(`Event '${val}' is not in the future.`);
        }

        throw new ArgumentParsingError(`Could not parse event from '${val}'.`);
    }
}

export = FutureEventArgumentType;
