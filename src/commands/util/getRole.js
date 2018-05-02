'use strict';

const { GlobalPaths } = require('globalobjects');

const Format = require(GlobalPaths.DiscordFormatter);
const Command = require(GlobalPaths.Command);
const EmbedUtil = require(GlobalPaths.EmbedUtil);

class GetRoleCommand extends Command {
    constructor(client) {
        super(client, {
            name: 'getrole',
            aliases: ['gr'],
            group: 'util',
            memberName: 'getrole',
            description: 'Gives you an accessible role.',
            examples: ['getrole @tour', 'gr leaks'],
            guildOnly: true,

            args: [
                {
                    key: 'role',
                    label: 'role',
                    type: 'role',
                    prompt: 'What role would you like to get?'
                }
            ]
        });
    }

    async run(message, { role }) {
        const { member } = message;
        const { database } = this.client;
        const specialRole = await database.specialRoles.get(role);

        if (specialRole && specialRole.accessible) {
            await member.edit({ 'roles': [role] }, 'Gave Role to user as per GetRole Command');
            return this.client.sendEmbed(message.channel,
                EmbedUtil.success(`Gave role '${Format.role(role, '#name')}' to ${member}.`));
        }
        else {
            return this.client.sendEmbed(message.channel,
                EmbedUtil.error(`Role ${Format.role(role, '#name (`#id`)')} is not marked as accessible.`));
        }
    }
}

module.exports = GetRoleCommand;