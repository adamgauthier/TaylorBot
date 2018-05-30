'use strict';

const { Paths } = require('globalobjects');

const ArgumentType = require(Paths.ArgumentType);
const ArgumentParsingError = require(Paths.ArgumentParsingError);

class ChannelArgumentType extends ArgumentType {
    constructor() {
        super({
            includesSpaces: true
        });
    }

    get id() {
        return 'channel';
    }

    parse(val, { message }) {
        const { guild, member } = message;

        if (member) {
            const matches = val.match(/^(?:<#)?([0-9]+)>?$/);
            if (matches) {
                const channel = guild.channels.resolve(matches[1]);
                if (channel && channel.permissionsFor(member).has('VIEW_CHANNEL')) {
                    return channel;
                }
            }

            const search = val.toLowerCase();
            const channels = guild.channels.filter(ChannelArgumentType.channelFilterInexact(search));
            if (channels.size === 0) {
                throw new ArgumentParsingError(`Could not find channel '${val}'.`);
            }
            else if (channels.size === 1) {
                const channel = channels.first();
                if (channel.permissionsFor(member).has('VIEW_CHANNEL')) {
                    return channel;
                }
                else {
                    throw new ArgumentParsingError(`Could not find channel '${val}' that you can view.`);
                }
            }

            const exactChannels = channels.filter(ChannelArgumentType.channelFilterExact(search));

            for (const channel of exactChannels.values()) {
                if (channel.permissionsFor(member).has('VIEW_CHANNEL')) {
                    return channel;
                }
            }

            for (const channel of channels.values()) {
                if (channel.permissionsFor(member).has('VIEW_CHANNEL')) {
                    return channel;
                }
            }
        }

        throw new ArgumentParsingError(`Could not find channel '${val}' that you can view.`);
    }

    static channelFilterExact(search) {
        return channel => channel.name.toLowerCase() === search;
    }

    static channelFilterInexact(search) {
        return channel => channel.name.toLowerCase().includes(search);
    }
}

module.exports = ChannelArgumentType;
