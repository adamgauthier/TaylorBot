import { CommandError } from '../../commands/CommandError';
import { AgePresenter } from '../user-presenters/AgePresenter.js';
import { SettableUserAttribute } from '../SettableUserAttribute.js';
import { DatabaseDriver } from '../../database/DatabaseDriver.js';
import { User, MessageEmbed } from 'discord.js';
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
        const age = await database.integerAttributes.get(this.id, user);
        const birthday = await database.birthdays.get(user);

        if (!age && !birthday) {
            return null;
        }

        return {
            age,
            birthday
        };
    }

    setCommand(commandContext: CommandMessageContext, _: any): Promise<MessageEmbed> {
        const setCommand = commandContext.client.master.registry.commands.getCommand(`setbirthday`);
        const context = new CommandMessageContext(commandContext.messageContext, setCommand);

        throw new CommandError([
            `Setting age directly is no longer supported, please use \`${context.usage()}\`. ⚠`,
            `This way, your age will automatically update and you will get points on your birthday every year! 🎈`,
            `If you don't want to share your exact birthday but still want the points as well as horoscope and age commands, use \`setprivatebirthday\` in DMs. 🕵️‍`
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
