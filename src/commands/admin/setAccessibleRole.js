'use strict';

const { GlobalPaths } = require('globalobjects');

const UserGroups = require(GlobalPaths.UserGroups);
const Format = require(GlobalPaths.DiscordFormatter);
const Command = require(GlobalPaths.Command);
const EmbedUtil = require(GlobalPaths.EmbedUtil);

class SetAccessibleRoleCommand extends Command {
    constructor(client) {
        super(client, {
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

    async run(message, { role }) {
        const { database } = this.client.master;
        const specialRole = await database.specialRoles.get(role);

        if (specialRole && specialRole.accessible) {
            return this.client.sendEmbed(message.channel,
                EmbedUtil.error(`Role ${Format.role(role, '#name (`#id`)')} is already accessible.`));
        }
        else {
            await database.specialRoles.setAccessible(role);
            return this.client.sendEmbed(message.channel,
                EmbedUtil.success(`Successfully made role ${Format.role(role, '#name (`#id`)')} accessible to anyone.`));
        }
    }
}

module.exports = SetAccessibleRoleCommand;