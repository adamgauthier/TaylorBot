'use strict';

const CommandError = require('../../commands/CommandError.js');
const SettableUserAttribute = require('../SettableUserAttribute.js');
const CommandMessageContext = require('../../commands/CommandMessageContext.js');
const AgePresentor = require('../user-presentors/AgePresentor.js');

class AgeAttribute extends SettableUserAttribute {
    constructor() {
        super({
            id: 'age',
            description: 'age',
            value: {
                label: 'age',
                type: 'age',
                example: '22'
            },
            presentor: AgePresentor
        });
    }

    async retrieve(database, user) {
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

    setCommand(commandContext) {
        const setCommand = commandContext.client.master.registry.commands.resolve(`setbirthday`);
        const context = new CommandMessageContext(commandContext.messageContext, setCommand);

        throw new CommandError([
            `Setting age directly is no longer supported, please use \`${context.usage()}\`. ⚠`,
            `This way, your age will automatically update and you will get points on your birthday every year! 🎈`
        ].join('\n'));
    }

    clear(database, user) {
        return database.integerAttributes.clear(this.id, user);
    }
}

module.exports = AgeAttribute;