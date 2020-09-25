import TextArgumentType = require('../base/Text');
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

const males = [
    'm', 'male', 'boy', 'man'
];
const females = [
    'f', 'female', 'girl', 'woman'
];

class GenderArgumentType extends TextArgumentType {
    get id(): string {
        return 'gender';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): 'Male' | 'Female' | 'Other' {
        const gender = val.trim().toLowerCase();

        if (males.includes(gender))
            return 'Male';

        if (females.includes(gender))
            return 'Female';

        return 'Other';
    }
}

export = GenderArgumentType;
