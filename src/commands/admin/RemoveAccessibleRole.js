'use strict';

const UserGroups = require('../../client/UserGroups.json');
const Format = require('../../modules/DiscordFormatter.js');
const Command = require('../../structures/Command.js');
const CommandError = require('../../structures/CommandError.js');

class RemoveAccessibleRoleCommand extends Command {
    constructor() {
        super({
            name: 'removeaccessiblerole',
            aliases: ['rar'],
            group: 'admin',
            description: 'Makes an accessible role inaccessible for users to get.',
            minimumGroup: UserGroups.Moderators,
            examples: ['removeaccessiblerole @tour', 'rar leaks'],
            guildOnly: true,

            args: [
                {
                    key: 'role',
                    label: 'role',
                    type: 'role',
                    prompt: 'What role would you like to make inaccessible again?'
                }
            ]
        });
    }

    async run({ message, client }, { role }) {
        const { database } = client.master;
        const specialRole = await database.specialRoles.get(role);

        if (!specialRole || !specialRole.accessible) {
            throw new CommandError(`Role ${Format.role(role, '#name (`#id`)')} is already inaccessible.`);
        }

        await database.specialRoles.removeAccessible(role);
        return client.sendEmbedSuccess(message.channel, `Successfully made role ${Format.role(role, '#name (`#id`)')} inaccessible.`);
    }
}

module.exports = RemoveAccessibleRoleCommand;