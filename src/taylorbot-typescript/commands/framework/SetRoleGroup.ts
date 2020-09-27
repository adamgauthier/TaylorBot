import { Role } from 'discord.js';
import UserGroups = require('../../client/UserGroups.js');
import Format = require('../../modules/DiscordFormatter.js');
import { Command } from '../Command';
import { CommandError } from '../CommandError';
import { CommandMessageContext } from '../CommandMessageContext';

class SetRoleGroupCommand extends Command {
    constructor() {
        super({
            name: 'setrolegroup',
            aliases: ['srg'],
            group: 'framework',
            description: 'Attaches a user group to a role.',
            minimumGroup: UserGroups.GuildManagers,
            examples: ['@admins Moderators', 'owners Moderators'],
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
        });
    }

    async run({ message, client }: CommandMessageContext, { role, group }: { role: Role; group: { name: string; accessLevel: number }  }): Promise<void> {
        const { roleGroups } = client.master.registry;

        if (roleGroups.getRoleGroup(role, group)) {
            throw new CommandError(`User Group '${group.name}' is already attached to Role ${Format.role(role, '#name (`#id`)')}.`);
        }

        await roleGroups.addRoleGroup(role, group);
        await client.sendEmbedSuccess(message.channel, `Attached User Group '${group.name}' to Role ${Format.role(role, '#name (`#id`)')}.`);
    }
}

export = SetRoleGroupCommand;
