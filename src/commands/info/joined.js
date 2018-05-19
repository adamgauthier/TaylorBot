'use strict';

const { GlobalPaths } = require('globalobjects');

const DiscordEmbedFormatter = require(GlobalPaths.DiscordEmbedFormatter);
const Command = require(GlobalPaths.Command);
const MathUtil = require(GlobalPaths.MathUtil);
const TimeUtil = require(GlobalPaths.TimeUtil);

class JoinedCommand extends Command {
    constructor() {
        super({
            name: 'joined',
            group: 'info',
            memberName: 'joined',
            description: 'Gets the first joined date of a user in the current server.',
            examples: ['joined @Enchanted13#1989', 'joined Enchanted13'],
            guildOnly: true,
            argsPromptLimit: 0,

            args: [
                {
                    key: 'member',
                    label: 'user',
                    type: 'member-or-author',
                    prompt: 'What user would you like to see the first joined date of?'
                }
            ]
        });
    }

    async run({ message, client }, { member }) {
        const { first_joined_at, rank } = await client.master.database.guildMembers.getRankedFirstJoinedAt(member);
        const embed = DiscordEmbedFormatter
            .baseUserHeader(member.user)
            .setDescription(
                `${member.displayName} first joined the server on **${TimeUtil.formatFull(first_joined_at)}**.\n` +
                `They were the **${MathUtil.formatNumberSuffix(rank)}** user to join.`
            );
        return client.sendEmbed(message.channel, embed);
    }
}

module.exports = JoinedCommand;