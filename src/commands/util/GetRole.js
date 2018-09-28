'use strict';

const Format = require('../../modules/DiscordFormatter.js');
const Command = require('../Command.js');
const CommandError = require('../CommandError.js');

class GetRoleCommand extends Command {
    constructor() {
        super({
            name: 'getrole',
            aliases: ['gr'],
            group: 'util',
            description: 'Gives you an accessible role.',
            examples: ['@tour', 'leaks'],
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

    async run({ message, client }, { role }) {
        const { member } = message;

        if (member.roles.has(role.id)) {
            throw new CommandError(`${member} already has role '${Format.role(role, '#name (`#id`)')}'.`);
        }

        const specialRole = await client.master.database.specialRoles.get(role);

        if (!specialRole || !specialRole.accessible) {
            throw new CommandError(`Role '${Format.role(role, '#name (`#id`)')}' is not marked as accessible.`);
        }

        await member.roles.add(role, `Gave accessible role ${Format.role(role)} to ${Format.user(message.author)} as per GetRole Command`);

        return client.sendEmbedSuccess(message.channel, `Gave role '${Format.role(role, '#name')}' to ${member}.`);
    }
}

module.exports = GetRoleCommand;