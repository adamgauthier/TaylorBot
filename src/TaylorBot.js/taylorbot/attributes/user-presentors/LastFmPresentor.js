'use strict';

const LastFmModule = require('../../modules/lastfm/LastFmModule.js');
const CommandError = require('../../commands/CommandError.js');
const DiscordEmbedFormatter = require('../../modules/DiscordEmbedFormatter.js');

class LastFmPresentor {
    constructor(attribute) {
        this.attribute = attribute;
    }

    async present(commandContext, user, attribute) {
        const response = await LastFmModule.getRecentTracks(attribute.attribute_value, 1);

        if (response.error)
            throw new CommandError(`An error occurred when contacting Last.fm: ${response.message}`);

        const { recenttracks: recentTracks } = response;

        const lastFmUser = recentTracks['@attr'];

        const embed = DiscordEmbedFormatter.baseUserEmbed(user);

        embed.author.name = lastFmUser.user;
        embed.author.url = `https://www.last.fm/user/${lastFmUser.user}`;

        const mostRecentTrack = recentTracks.track[0];

        if (!mostRecentTrack) {
            return embed.setDescription(`Last.fm user '${lastFmUser.user}' doesn't have any scrobbles.`);
        }

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
}

module.exports = LastFmPresentor;