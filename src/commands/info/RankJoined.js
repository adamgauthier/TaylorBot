'use strict';

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const Command = require('../../structures/Command.js');
const TimeUtil = require('../../modules/TimeUtil.js');
const ArrayUtil = require('../../modules/ArrayUtil.js');
const ArrayEmbedDescriptionPageMessage = require('../../modules/paging/ArrayEmbedDescriptionPageMessage.js');

class RankJoinedCommand extends Command {
    constructor() {
        super({
            name: 'rankjoined',
            group: 'info',
            description: 'Gets the list of users that joined the current server first.',
            examples: ['rankjoined'],
            guildOnly: true,

            args: [
                {
                    key: 'guild',
                    label: 'server',
                    prompt: 'What server would you like to see the join list of?',
                    type: 'guild-or-current'
                }
            ]
        });
    }

    async run({ message, client }, { guild }) {
        const members = await client.master.database.guildMembers.getRankedFirstJoinedAt(guild, 100);

        const embed = DiscordEmbedFormatter.baseGuildHeader(guild);
        const lines = members.map(m => `${m.rank}: <@${m.user_id}> - ${TimeUtil.formatMini(m.first_joined_at)}`);

        return new ArrayEmbedDescriptionPageMessage(
            client,
            message.author,
            embed,
            ArrayUtil.chunk(lines, 20).map(chunk => chunk.join('\n'))
        ).send(message.channel);
    }
}

module.exports = RankJoinedCommand;