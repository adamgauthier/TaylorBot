'use strict';

const { GlobalPaths } = require('globalobjects');

const UserGroups = require(GlobalPaths.UserGroups);
const Format = require(GlobalPaths.DiscordFormatter);
const Command = require(GlobalPaths.Command);

class SetRoleGroupCommand extends Command {
    constructor(client) {
        super(client, {
            name: 'setrolegroup',
            aliases: ['srg'],
            group: 'admin',
            memberName: 'setrolegroup',
            description: 'Attaches a user group to a role.',
            examples: ['setrolegroup @admins Moderators', 'srg owners Moderators'],
            guildOnly: true,

            args: [
                {
                    key: 'role',
                    label: 'role',
                    type: 'role',
                    prompt: 'What role would you like to attach a group to?'
                },
                {
                    key: 'group',
                    label: 'group',
                    type: 'user-group',
                    prompt: 'What group would you like to attach to the role?'
                }
            ]
        }, UserGroups.Master);
    }

    async run(message, { role, group }) {
        const { roleGroups } = this.client.oldRegistry;
        if (roleGroups.getRoleGroup(role, group)) {
            return this.client.sendMessage(message.channel, `User Group '${group.name}' is already attached to Role ${Format.role(role, '#name (`#id`)')}.`);
        }
        else {
            await roleGroups.addRoleGroup(role, group);
            return this.client.sendMessage(message.channel, `Attached User Group '${group.name}' to Role ${Format.role(role, '#name (`#id`)')}.`);
        }
    }
}

module.exports = SetRoleGroupCommand;