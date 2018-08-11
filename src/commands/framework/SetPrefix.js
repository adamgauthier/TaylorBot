'use strict';

const { Paths } = require('globalobjects');

const UserGroups = require(Paths.UserGroups);
const Format = require(Paths.DiscordFormatter);
const Command = require(Paths.Command);
const CommandError = require('../../structures/CommandError.js');

class SetPrefixCommand extends Command {
    constructor() {
        super({
            name: 'setprefix',
            aliases: ['sp', 'prefix'],
            group: 'framework',
            description: `Changes the bot's prefix for a server.`,
            minimumGroup: UserGroups.Moderators,
            examples: ['setprefix .', 'sp !'],
            guildOnly: true,
            guarded: true,

            args: [
                {
                    key: 'prefix',
                    label: 'prefix',
                    type: 'word',
                    prompt: 'What would you like to set the prefix to?'
                }
            ]
        });
    }

    async run({ message, client }, { prefix }) {
        const { guilds } = client.master.registry;
        const { guild } = message;

        const currentPrefix = guilds.get(guild.id).prefix;

        if (currentPrefix === prefix) {
            throw new CommandError(`The prefix for ${Format.guild(guild, '#name (`#id`)')} is already '${currentPrefix}'.`);
        }

        await guilds.changePrefix(guild, prefix);

        return client.sendEmbedSuccess(message.channel, `Changed prefix for ${Format.guild(guild, '#name (`#id`)')} to '${prefix}'.`);
    }
}

module.exports = SetPrefixCommand;