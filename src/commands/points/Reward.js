'use strict';

const UserGroups = require('../../client/UserGroups.json');
const Command = require('../Command.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const StringUtil = require('../../modules/StringUtil.js');

class RewardCommand extends Command {
    constructor() {
        super({
            name: 'reward',
            group: 'points',
            description: 'Rewards a specified amount of taypoints to pinged users.',
            minimumGroup: UserGroups.Master,
            examples: ['13 @Enchanted13#1989', '22 @Enchanted13#1989 @Lydia#7147'],

            args: [
                {
                    key: 'amount',
                    label: 'taypoints',
                    type: 'strictly-positive-integer',
                    prompt: 'How much taypoints do you want to reward each user?'
                },
                {
                    key: 'users',
                    label: 'users',
                    type: 'mentioned-users',
                    prompt: 'What users would you like to reward taypoints to (must be mentioned)?'
                }
            ]
        });
    }

    async run({ message, client }, { amount, users }) {
        const { author, channel } = message;
        await client.master.database.users.addTaypointCount(users, amount);

        return client.sendEmbed(channel, DiscordEmbedFormatter
            .baseUserEmbed(author)
            .setDescription([
                `Successfully rewarded ${StringUtil.plural(amount, 'taypoint', '**')} to:`,
                ...users.map(user => `${user}`)
            ].join('\n'))
        );
    }
}

module.exports = RewardCommand;