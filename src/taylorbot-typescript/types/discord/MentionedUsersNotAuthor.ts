import ArgumentParsingError = require('../ArgumentParsingError.js');
import { CommandMessageContext, CommandArgumentInfo } from '../../commands/CommandMessageContext';
import { User } from 'discord.js';
import TextArgumentType = require('../base/Text.js');
import MentionedUserArgumentType = require('./MentionedUser.js');

class MentionedUsersNotAuthorArgumentType extends TextArgumentType {
    #mentionedUserArgumentType: MentionedUserArgumentType = new MentionedUserArgumentType();

    get id(): string {
        return 'mentioned-users-not-author';
    }

    async parse(val: string, commandContext: CommandMessageContext, arg: CommandArgumentInfo): Promise<User[]> {
        const mentions = val.split(' ').filter(mention => mention !== '');

        const users = await Promise.all(
            mentions.map(mention => this.#mentionedUserArgumentType.parse(mention, commandContext, arg))
        );

        if (users.some(user => user.id === commandContext.message.author!.id)) {
            throw new ArgumentParsingError(`You can't mention yourself.`);
        }

        return users;
    }
}

export = MentionedUsersNotAuthorArgumentType;
