'use strict';

const { Paths } = require('globalobjects');

const Format = require(Paths.DiscordFormatter);
const Command = require(Paths.Command);
const CommandError = require('../../structures/CommandError.js');

class DropRoleCommand extends Command {
    constructor() {
        super({
            name: 'droprole',
            aliases: ['dr'],
            group: 'util',
            description: 'Removes an accessible role from you.',
            examples: ['droprole @tour', 'dr leaks'],
            guildOnly: true,

            args: [
                {
                    key: 'role',
                    label: 'role',
                    type: 'role',
                    prompt: 'What role would you like to be dropped?'
                }
            ]
        });
    }

    async run({ message, client }, { role }) {
        const { member } = message;

        if (!member.roles.has(role.id)) {
            throw new CommandError(`${member} doesn't have role '${Format.role(role, '#name (`#id`)')}'.`);
        }

        const specialRole = await client.master.database.specialRoles.get(role);

        if (!specialRole || !specialRole.accessible) {
            throw new CommandError(`Role '${Format.role(role, '#name (`#id`)')}' is not marked as accessible.`);
        }

        await member.roles.remove(role, `Removed accessible role ${Format.role(role)} from ${Format.user(message.author)} as per DropRole Command`);

        return client.sendEmbedSuccess(message.channel, `Removed role '${Format.role(role, '#name')}' from ${member}.`);
    }
}

module.exports = DropRoleCommand;