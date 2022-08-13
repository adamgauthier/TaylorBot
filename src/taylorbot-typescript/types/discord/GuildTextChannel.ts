import { BaseGuildTextChannel, ChannelType, GuildChannel, ThreadChannel } from 'discord.js';
import WordArgumentType = require('../base/Word');
import ChannelArgumentType = require('./Channel.js');
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

class GuildTextChannelArgumentType extends WordArgumentType {
    readonly #channelArgumentType = new ChannelArgumentType({
        channelFilter: (channel: GuildChannel | ThreadChannel): boolean => channel.type === ChannelType.GuildText || channel.type === ChannelType.GuildNews || channel.isThread()
    });

    get id(): string {
        return 'guild-text-channel';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): BaseGuildTextChannel | ThreadChannel {
        return this.#channelArgumentType.parse(val, commandContext, arg) as BaseGuildTextChannel | ThreadChannel;
    }
}

export = GuildTextChannelArgumentType;
