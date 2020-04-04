'use strict';

const moment = require('moment');
const querystring = require('querystring');

const Command = require('../Command.js');
const CommandError = require('../CommandError.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');

class LastFmCollageCommand extends Command {
    constructor() {
        super({
            name: 'lastfmcollage',
            aliases: ['fmcollage', 'fmc'],
            group: 'fm',
            description: 'Generates a collage based on your Last.Fm listening habits.',
            examples: ['7d 3x3', 'overall 4x4'],

            args: [
                {
                    key: 'period',
                    label: 'period',
                    type: 'last-fm-period',
                    prompt: 'What period of time would you like your collage for?'
                },
                {
                    key: 'size',
                    label: 'size',
                    type: 'last-fm-size',
                    prompt: 'What size (number of rows and columns) would you like your collage to be?'
                }
            ]
        });
    }

    async run({ message, client }, { period, size }) {
        const user = message.author;
        const attribute = await client.master.database.textAttributes.get('lastfm', user);
        if (!attribute) {
            throw new CommandError(`${user.username}'s Last.fm username is not set. They can use the \`setlastfm\` command to set it.`);
        }

        return client.sendEmbed(message.channel, DiscordEmbedFormatter
            .baseUserEmbed(user)
            .setTitle(`Last.fm ${size}x${size} collage for period '${period}'`)
            .setImage(`https://lastfmtopalbums.dinduks.com/patchwork.php?${querystring.stringify({
                user: attribute.attribute_value,
                period,
                rows: size,
                cols: size,
                imageSize: 400,
                a: moment.utc().endOf('day').format('X')
            })}`)
        );
    }
}

module.exports = LastFmCollageCommand;