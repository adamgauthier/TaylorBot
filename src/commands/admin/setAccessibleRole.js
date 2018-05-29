'use strict';

const { Paths } = require('globalobjects');

const UserGroups = require(Paths.UserGroups);
const Format = require(Paths.DiscordFormatter);
const Command = require(Paths.Command);
const CommandError = require(Paths.CommandError);

class SetAccessibleRoleCommand extends Command {
    constructor() {
        super({
            name: 'setaccessiblerole',
            aliases: ['sar'],
            group: 'admin',
            description: 'Makes a role accessible for users to get.',
            minimumGroup: UserGroups.Moderators,
            examples: ['setaccessiblerole @tour', 'sar leaks'],
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

module.exports = SetAccessibleRoleCommand;