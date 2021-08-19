import { BaseGuildTextChannel, GuildChannel, ThreadChannel } from 'discord.js';
import WordArgumentType = require('../base/Word');
import ChannelArgumentType = require('./Channel.js');
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

class GuildTextChannelArgumentType extends WordArgumentType {
    readonly #channelArgumentType = new ChannelArgumentType({
        channelFilter: (channel: GuildChannel | ThreadChannel): boolean => channel.type === 'GUILD_TEXT' || channel.type === 'GUILD_NEWS' || channel.isThread()
    });

    get id(): string {
        return 'guild-text-channel';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): BaseGuildTextChannel | ThreadChannel {
        return this.#channelArgumentType.parse(val, commandContext, arg) as BaseGuildTextChannel | ThreadChannel;
    }
}

export = GuildTextChannelArgumentType;
