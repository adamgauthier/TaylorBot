'use strict';

const { GlobalPaths } = require('globalobjects');

const ArgumentType = require(GlobalPaths.ArgumentType);
const ArgumentParsingError = require(GlobalPaths.ArgumentParsingError);

class ChannelArgumentType extends ArgumentType {
    constructor() {
        super('channel');
    }

    parse(val, message) {
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
            const channels = guild.channels.filterArray(ChannelArgumentType.channelFilterInexact(search));
            if (channels.length === 0) {
                throw new ArgumentParsingError(`Could not find channel '${val}'.`);
            }
            else if (channels.length === 1) {
                if (channels[0].permissionsFor(member).has('VIEW_CHANNEL')) {
                    return channels[0];
                }
                else {
                    throw new ArgumentParsingError(`Could not find channel '${val}' that you can view.`);
                }
            }

            const exactChannels = channels.filter(ChannelArgumentType.channelFilterExact(search));

            for (const channel of exactChannels) {
                if (channel.permissionsFor(member).has('VIEW_CHANNEL')) {
                    return channel;
                }
            }

            for (const channel of channels) {
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
