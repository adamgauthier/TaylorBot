'use strict';

const { Paths } = require('globalobjects');

const UserGroups = require(Paths.UserGroups);
const Format = require(Paths.DiscordFormatter);
const Command = require(Paths.Command);
const EmbedUtil = require(Paths.EmbedUtil);

class SetAccessibleRoleCommand extends Command {
    constructor() {
        super({
            name: 'setaccessiblerole',
            aliases: ['sar'],
            group: 'admin',
            memberName: 'setaccessiblerole',
            description: 'Makes a role accessible for users to get.',
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
        }, UserGroups.Moderators);
    }

    async run({ message, client }, { role }) {
        const { database } = client.master;
        const specialRole = await database.specialRoles.get(role);

        if (specialRole && specialRole.accessible) {
            return client.sendEmbed(message.channel,
                EmbedUtil.error(`Role ${Format.role(role, '#name (`#id`)')} is already accessible.`));
        }
        else {
            await database.specialRoles.setAccessible(role);
            return client.sendEmbed(message.channel,
                EmbedUtil.success(`Successfully made role ${Format.role(role, '#name (`#id`)')} accessible to anyone.`));
        }
    }
}

module.exports = SetAccessibleRoleCommand;