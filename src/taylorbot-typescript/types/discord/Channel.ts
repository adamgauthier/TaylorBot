import TextArgumentType = require('../base/Text');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';
import { GuildChannel, PermissionFlagsBits, ThreadChannel } from 'discord.js';

class ChannelArgumentType extends TextArgumentType {
    readonly channelFilter: (channel: GuildChannel | ThreadChannel) => boolean;

    constructor({ channelFilter = (channel: GuildChannel | ThreadChannel): boolean => true } = {}) {
        super();
        this.channelFilter = channelFilter;
    }

    get id(): string {
        return 'channel';
    }

    parse(val: string, { message }: CommandMessageContext, arg: CommandArgumentInfo): GuildChannel | ThreadChannel {
        const { guild, member } = message;

        if (member && guild) {
            const matches = val.trim().match(/^(?:<#)?([0-9]+)>?$/);
            if (matches) {
                const channel = guild.channels.resolve(matches[1]);
                if (channel && channel.permissionsFor(member).has(PermissionFlagsBits.ViewChannel)) {
                    return channel;
                }
            }

            const search = val.toLowerCase();
            const channels = guild.channels.cache.filter(this.channelFilterInexact(search));
            if (channels.size === 0) {
                throw new ArgumentParsingError(`Could not find channel '${val}'.`);
            }
            else if (channels.size === 1) {
                const channel = channels.first()!;
                if (channel.permissionsFor(member).has(PermissionFlagsBits.ViewChannel)) {
                    return channel;
                }
                else {
                    throw new ArgumentParsingError(`Could not find channel '${val}' that you can view.`);
                }
            }

            const exactChannels = channels.filter(this.channelFilterExact(search));

            for (const channel of exactChannels.values()) {
                if (channel.permissionsFor(member).has(PermissionFlagsBits.ViewChannel)) {
                    return channel;
                }
            }

            for (const channel of channels.values()) {
                if (channel.permissionsFor(member).has(PermissionFlagsBits.ViewChannel)) {
                    return channel;
                }
            }
        }

        throw new ArgumentParsingError(`Could not find channel '${val}' that you can view.`);
    }

    channelFilterExact(search: string) {
        return (channel: GuildChannel | ThreadChannel): boolean => channel.name.toLowerCase() === search && this.channelFilter(channel);
    }

    channelFilterInexact(search: string) {
        return (channel: GuildChannel | ThreadChannel): boolean => channel.name.toLowerCase().includes(search);
    }
}

export = ChannelArgumentType;
