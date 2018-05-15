'use strict';

const { GlobalPaths } = require('globalobjects');

const ArgumentType = require(GlobalPaths.ArgumentType);

const channelFilterExact = search => {
    return channel => channel.name.toLowerCase() === search;
};

const channelFilterInexact = search => {
    return channel => channel.name.toLowerCase().includes(search);
};

class ChannelArgumentType extends ArgumentType {
    constructor(client) {
        super(client, 'channel2');
    }

    get id() {
        return 'channel2';
    }

    validate(val, msg) {
        if (msg.guild) {
            const matches = val.match(/^(?:<#)?([0-9]+)>?$/);
            if (matches) {
                const channel = msg.guild.channels.resolve(matches[1]);
                if (channel && channel.permissionsFor(msg.member).has('VIEW_CHANNEL')) {
                    return true;
                }
            }

            const search = val.toLowerCase();
            const channels = msg.guild.channels.filterArray(channelFilterInexact(search));
            if (channels.length === 0)
                return false;
            if (channels.length === 1) {
                if (channels[0].permissionsFor(msg.member).has('VIEW_CHANNEL')) {
                    return true;
                }
            }

            const exactChannels = channels.filter(channelFilterExact(search));
            if (exactChannels.length > 0) {
                for (const channel of exactChannels) {
                    if (channel.permissionsFor(msg.member).has('VIEW_CHANNEL')) {
                        return true;
                    }
                }
            }
            else {
                for (const channel of channels) {
                    if (channel.permissionsFor(msg.member).has('VIEW_CHANNEL')) {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    parse(val, msg) {
        if (msg.guild) {
            const matches = val.match(/^(?:<#)?([0-9]+)>?$/);
            if (matches) {
                const channel = msg.guild.channels.resolve(matches[1]);
                if (channel && channel.permissionsFor(msg.member).has('VIEW_CHANNEL')) {
                    return channel;
                }
            }

            const search = val.toLowerCase();
            const channels = msg.guild.channels.filterArray(channelFilterInexact(search));
            if (channels.length === 0)
                return null;
            if (channels.length === 1) {
                if (channels[0].permissionsFor(msg.member).has('VIEW_CHANNEL')) {
                    return channels[0];
                }
            }

            const exactChannels = channels.filter(channelFilterExact(search));
            if (exactChannels.length > 0) {
                for (const channel of exactChannels) {
                    if (channel.permissionsFor(msg.member).has('VIEW_CHANNEL')) {
                        return channel;
                    }
                }
            }
            else {
                for (const channel of channels) {
                    if (channel.permissionsFor(msg.member).has('VIEW_CHANNEL')) {
                        return channel;
                    }
                }
            }
        }

        return null;
    }
}

module.exports = ChannelArgumentType;
