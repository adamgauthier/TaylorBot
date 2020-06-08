'use strict';

const Command = require('../Command.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');

class ServerStatsCommand extends Command {
    constructor() {
        super({
            name: 'serverstats',
            aliases: ['sstats', 'genderstats', 'agestats'],
            group: 'Stats 📊',
            description: 'Gets age and gender stats for a server.',
            examples: [''],

            args: [
                {
                    key: 'guild',
                    label: 'server',
                    prompt: 'What server would you like to see the stats of?',
                    type: 'guild-or-current'
                }
            ]
        });
    }

    async run({ message: { channel }, client }, { guild }) {
        const { avg, median } = await client.master.database.birthdays.getAgeStatsInGuild(guild);
        const genderAttributes = await client.master.database.textAttributes.getInGuild('gender', guild);

        const males = genderAttributes.filter(a => a.attribute_value === 'Male').length;
        const females = genderAttributes.filter(a => a.attribute_value === 'Female').length;
        const others = genderAttributes.filter(a => a.attribute_value === 'Other').length;

        return client.sendEmbed(channel, DiscordEmbedFormatter
            .baseGuildHeader(guild)
            .addField('Age', [
                `Median: ${median != null ? median : 'No Data'}`,
                `Average: ${avg != null ? avg : 'No Data'}`
            ].join('\n'), true)
            .addField('Gender', [
                `Male: ${males} (${(males / genderAttributes.length * 100).toFixed(2)}%)`,
                `Female: ${females} (${(females / genderAttributes.length * 100).toFixed(2)}%)`,
                `Other: ${others} (${(others / genderAttributes.length * 100).toFixed(2)}%)`
            ].join('\n'), true)
        );
    }
}

module.exports = ServerStatsCommand;
