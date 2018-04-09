'use strict';

const { GlobalPaths } = require('globalobjects');

const ArgumentInfos = require(GlobalPaths.ArgumentInfos);
const DiscordEmbedFormatter = require(GlobalPaths.DiscordEmbedFormatter);
const Command = require(GlobalPaths.Command);
const MathUtil = require(GlobalPaths.MathUtil);
const TimeUtil = require(GlobalPaths.TimeUtil);

class JoinedCommand extends Command {
    constructor(client) {
        super(client, {
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
                    ...ArgumentInfos.MemberOrAuthor
                }
            ]
        });
    }

    async run(message, { member }) {
        const { first_joined_at, rank } = await this.client.database.getRankedFirstJoinedAt(member);
        const embed = 
            DiscordEmbedFormatter.baseUserHeader(member.user)
            .setDescription(
                `${member.displayName} first joined the server on **${TimeUtil.formatFull(first_joined_at)}**.\n` +
                `They were the **${MathUtil.formatNumberSuffix(rank)}** user to join.`
            );
        return this.client.sendEmbed(message.channel, embed);
    }
}

module.exports = JoinedCommand;