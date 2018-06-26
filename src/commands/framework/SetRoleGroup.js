'use strict';

const { Paths } = require('globalobjects');

const UserGroups = require(Paths.UserGroups);
const Format = require(Paths.DiscordFormatter);
const Command = require(Paths.Command);
const CommandError = require(Paths.CommandError);

class SetRoleGroupCommand extends Command {
    constructor() {
        super({
            name: 'setrolegroup',
            aliases: ['srg'],
            group: 'framework',
            description: 'Attaches a user group to a role.',
            minimumGroup: UserGroups.GuildOwners,
            examples: ['setrolegroup @admins Moderators', 'srg owners Moderators'],
            guildOnly: true,
            guarded: true,

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
        });
    }

    async run({ message, client }, { role, group }) {
        const { roleGroups } = client.master.registry;

        if (roleGroups.getRoleGroup(role, group)) {
            throw new CommandError(`User Group '${group.name}' is already attached to Role ${Format.role(role, '#name (`#id`)')}.`);
        }

        await roleGroups.addRoleGroup(role, group);
        return client.sendEmbedSuccess(message.channel, `Attached User Group '${group.name}' to Role ${Format.role(role, '#name (`#id`)')}.`);
    }
}

module.exports = SetRoleGroupCommand;