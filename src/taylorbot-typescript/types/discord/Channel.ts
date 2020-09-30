import TextArgumentType = require('../base/Text');
import { ArgumentParsingError } from '../ArgumentParsingError';
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';
import { GuildChannel } from 'discord.js';

class ChannelArgumentType extends TextArgumentType {
    readonly channelFilter: (channel: GuildChannel) => boolean;

    constructor({ channelFilter = (channel: GuildChannel): boolean => true } = {}) {
        super();
        this.channelFilter = channelFilter;
    }

    get id(): string {
        return 'channel';
    }

    parse(val: string, { message }: CommandMessageContext, arg: CommandArgumentInfo): GuildChannel {
        const { guild, member } = message;

        if (member && guild) {
            const matches = val.trim().match(/^(?:<#)?([0-9]+)>?$/);
            if (matches) {
                const channel = guild.channels.resolve(matches[1]);
                if (channel && channel.permissionsFor(member)!.has('VIEW_CHANNEL')) {
                    return channel;
                }
            }

            const search = val.toLowerCase();
            const channels = guild.channels.cache.filter(this.channelFilterInexact(search));
            if (channels.size === 0) {
                throw new ArgumentParsingError(`Could not find channel '${val}'.`);
            }
            else if (channels.size === 1) {
                const channel = channels.first();
                if (channel!.permissionsFor(member)!.has('VIEW_CHANNEL')) {
                    return channel!;
                }
                else {
                    throw new ArgumentParsingError(`Could not find channel '${val}' that you can view.`);
                }
            }

            const exactChannels = channels.filter(this.channelFilterExact(search));

            for (const channel of exactChannels.values()) {
                if (channel.permissionsFor(member)!.has('VIEW_CHANNEL')) {
                    return channel;
                }
            }

            for (const channel of channels.values()) {
                if (channel.permissionsFor(member)!.has('VIEW_CHANNEL')) {
                    return channel;
                }
            }
        }

        throw new ArgumentParsingError(`Could not find channel '${val}' that you can view.`);
    }

    channelFilterExact(search: string) {
        return (channel: GuildChannel): boolean => channel.name.toLowerCase() === search && this.channelFilter(channel);
    }

    channelFilterInexact(search: string) {
        return (channel: GuildChannel): boolean => channel.name.toLowerCase().includes(search);
    }
}

export = ChannelArgumentType;
