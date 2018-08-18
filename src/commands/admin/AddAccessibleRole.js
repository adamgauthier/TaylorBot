'use strict';

const UserGroups = require('../../client/UserGroups.json');
const Format = require('../../modules/DiscordFormatter.js');
const Command = require('../../structures/Command.js');
const CommandError = require('../../structures/CommandError.js');

class AddAccessibleRoleCommand extends Command {
    constructor() {
        super({
            name: 'addaccessiblerole',
            aliases: ['aar'],
            group: 'admin',
            description: 'Makes a role accessible for users to get.',
            minimumGroup: UserGroups.Moderators,
            examples: ['addaccessiblerole @tour', 'aar leaks'],
            guildOnly: true,

            args: [
                {
                    key: 'role',
                    label: 'role',
                    type: 'role',
                    prompt: 'What role would you like to make accessible to anyone?'
                }
            ]
        });
    }

    async run({ message, client }, { role }) {
        const { database } = client.master;
        const specialRole = await database.specialRoles.get(role);

        if (specialRole && specialRole.accessible) {
            throw new CommandError(`Role ${Format.role(role, '#name (`#id`)')} is already accessible.`);
        }

        await database.specialRoles.setAccessible(role);
        return client.sendEmbedSuccess(message.channel, `Successfully made role ${Format.role(role, '#name (`#id`)')} accessible to anyone.`);
    }
}

module.exports = AddAccessibleRoleCommand;