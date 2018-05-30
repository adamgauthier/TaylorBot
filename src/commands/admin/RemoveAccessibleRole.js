'use strict';

const { Paths } = require('globalobjects');

const UserGroups = require(Paths.UserGroups);
const Format = require(Paths.DiscordFormatter);
const Command = require(Paths.Command);
const CommandError = require(Paths.CommandError);

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
        return client.sendEmbedSuccess(message.channel, `Successfully made role ${Format.role(role, '#name (`#id`)')} accessible to anyone.`);
    }
}

module.exports = RemoveAccessibleRoleCommand;