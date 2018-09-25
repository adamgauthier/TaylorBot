'use strict';

const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');
const TextUserAttribute = require('../TextUserAttribute.js');
const LastFmModule = require('../../modules/lastfm/LastFmModule.js');
const CommandError = require('../../structures/CommandError.js');

class LastFmAttribute extends TextUserAttribute {
    constructor() {
        super({
            id: 'lastfm',
            aliases: ['fm', 'np'],
            description: 'Last.fm username',
            value: {
                label: 'username',
                type: 'last-fm-username',
                example: 'taylorswift'
            },
            canList: true
        });
    }

    async getEmbed(commandContext, user, attribute) {
        const response = await LastFmModule.getRecentTracks(attribute, 1);

        if (response.error)
            throw new CommandError(`An error occurred when contacting Last.fm: ${response.message}`);

        const { recenttracks: recentTracks } = response;

        const lastFmUser = recentTracks['@attr'];

        const embed = DiscordEmbedFormatter.baseUserEmbed(user);

        embed.author.name = lastFmUser.user;
        embed.author.url = `https://www.last.fm/user/${lastFmUser.user}`;

        if (recentTracks.track.length === 0) {
            return embed.setDescription(`This user doesn't have any scrobbles.`);
        }

        const mostRecentTrack = recentTracks.track[0];
        const imageUrl = mostRecentTrack.image[2]['#text'] || mostRecentTrack.artist.image[2]['#text'];
        const isNowPlaying = mostRecentTrack['@attr'] && mostRecentTrack['@attr'].nowplaying;

        if (imageUrl)
            embed.setThumbnail(imageUrl);

        return embed
            .addField('Artist', `[${mostRecentTrack.artist.name}](${mostRecentTrack.artist.url.replace(')', '%29')})`, true)
            .addField('Track', `[${mostRecentTrack.name}](${mostRecentTrack.url.replace(')', '%29')})`, true)
            .setFooter([
                isNowPlaying ? 'Now Playing' : 'Most Recent Track',
                `Total Scrobbles: ${lastFmUser.total}`
            ].join(' | '), 'https://i.imgur.com/pVu9vTr.png');
    }

    format(attribute) {
        return `[${attribute}](https://www.last.fm/user/${attribute})`;
    }
}

module.exports = LastFmAttribute;