import { GuildChannel, TextChannel } from 'discord.js';
import WordArgumentType = require('../base/Word');
import ChannelArgumentType = require('./Channel.js');
import { CommandArgumentInfo, CommandMessageContext } from '../../commands/CommandMessageContext';

class GuildTextChannelArgumentType extends WordArgumentType {
    readonly #channelArgumentType = new ChannelArgumentType({
        channelFilter: (channel: GuildChannel): boolean => channel.type === 'text'
    });

    get id(): string {
        return 'guild-text-channel';
    }

    parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): TextChannel {
        return this.#channelArgumentType.parse(val, commandContext, arg) as TextChannel;
    }
}

export = GuildTextChannelArgumentType;
