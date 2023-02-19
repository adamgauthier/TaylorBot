import { CommandError } from '../../commands/CommandError';
import { AgePresenter } from '../user-presenters/AgePresenter.js';
import { SettableUserAttribute } from '../SettableUserAttribute.js';
import { DatabaseDriver } from '../../database/DatabaseDriver.js';
import { User, EmbedBuilder } from 'discord.js';
import { CommandMessageContext } from '../../commands/CommandMessageContext';

class AgeAttribute extends SettableUserAttribute {
    constructor() {
        super({
            id: 'age',
            aliases: [],
            description: 'age',
            value: {
                label: 'age',
                type: 'any-text',
                example: 'dont-use'
            },
            presenter: AgePresenter,
            list: null
        });
    }

    async retrieve(database: DatabaseDriver, user: User): Promise<any> {
        return {};
    }

    setCommand(commandContext: CommandMessageContext, _: any): Promise<EmbedBuilder> {
        throw new CommandError([
            `Setting age directly is no longer supported, please use </birthday set:1016938623880400907> with the **year** option. ⚠`,
            `This way, your age will automatically update and you will get points on your birthday every year! 🎈`,
            `If you don't want to share your exact birthday, but want the points, horoscope and age commands, use </birthday set:1016938623880400907> with the **privately** option. 🕵️‍`
        ].join('\n'));
    }

    set(database: DatabaseDriver, user: User, value: any): Promise<Record<string, any>> {
        throw new Error('Method not implemented.');
    }

    formatValue(attribute: Record<string, any>): string {
        throw new Error('Method not implemented.');
    }

    clear(database: DatabaseDriver, user: User): Promise<void> {
        return database.integerAttributes.clear(this.id, user);
    }
}

export = AgeAttribute;
