import PositiveSafeIntegerArgumentType = require('../numbers/PositiveSafeInteger.js');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { TaypointAmount } from '../../modules/points/TaypointAmount';
import StringUtil = require('../../modules/StringUtil.js');
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';
import WordArgumentType = require('../base/Word');

class TaypointAmountArgumentType extends WordArgumentType {
    readonly #positiveSafeIntegerArgumentType = new PositiveSafeIntegerArgumentType();

    get id(): string {
        return 'taypoint-amount';
    }

    async parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): Promise<TaypointAmount> {
        switch (val.toLowerCase()) {
            case 'all':
                return new TaypointAmount({ divisor: 1 });
            case 'half':
                return new TaypointAmount({ divisor: 2 });
            case 'third':
                return new TaypointAmount({ divisor: 3 });
            case 'fourth':
                return new TaypointAmount({ divisor: 4 });
        }

        const number = this.#positiveSafeIntegerArgumentType.parse(val, commandContext, arg);
        const { client, author } = commandContext;

        const { taypoint_count, has_enough } = await client.master.database.users.hasEnoughTaypointCount(author, number);

        if (!has_enough)
            throw new ArgumentParsingError(
                `You can't spend ${StringUtil.plural(number, 'taypoint', '**')}, you only have **${StringUtil.formatNumberString(taypoint_count)}**.`
            );

        return new TaypointAmount({ count: number });
    }
}

export = TaypointAmountArgumentType;
