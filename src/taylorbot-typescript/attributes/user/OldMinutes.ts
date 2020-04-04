import { UserAttribute } from '../UserAttribute';
import { SimplePresenter } from '../user-presenters/SimplePresenter';
import { DatabaseDriver } from '../../database/DatabaseDriver';
import { User } from 'discord.js';

class OldMinutesAttribute extends UserAttribute {
    constructor() {
        super({
            id: 'oldminutes',
            aliases: [],
            description: 'minutes spent online (before December 25th 2015)',
            presenter: SimplePresenter,
            list: null,
            canSet: false
        });
    }

    retrieve(database: DatabaseDriver, user: User): Promise<any> {
        return database.integerAttributes.get(this.id, user);
    }

    formatValue(attribute: Record<string, any>): string {
        return attribute.integer_value.toString();
    }
}

export = OldMinutesAttribute;
