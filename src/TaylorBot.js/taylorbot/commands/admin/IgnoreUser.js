'use strict';

const UserGroups = require('../../client/UserGroups.js');
const Command = require('../Command.js');
const TimeUtil = require('../../modules/TimeUtil.js');

class IgnoreUserCommand extends Command {
    constructor() {
        super({
            name: 'ignoreuser',
            aliases: ['iu'],
            group: 'admin',
            description: 'Makes a role accessible for users to get.',
            minimumGroup: UserGroups.Master,
            examples: ['@Enchanted13#1989 12 minutes'],

            args: [
                {
                    key: 'user',
                    label: 'user',
                    type: 'mentioned-user',
                    prompt: 'What users would you like me to ignore (must be mentioned)?'
                },
                {
                    key: 'event',
                    label: 'when',
                    type: 'future-event',
                    prompt: 'When should I stop ignoring this user?'
                }
            ]
        });
    }

    async run({ message, client }, { user, event }) {
        const { channel } = message;
        const { users } = client.master.registry;

        await users.ignoreUser(user, event.startDate);

        return client.sendEmbedSuccess(channel,
            `Okay, I will ignore ${user} until \`${TimeUtil.formatLog(event.startDate.valueOf())}\`. 😊`
        );
    }
}

module.exports = IgnoreUserCommand;