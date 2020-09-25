import { User } from 'discord.js';
import { CommandMessageContext } from '../../commands/CommandMessageContext';
import UserArgumentType = require('./User.js');

class UserOrAuthorArgumentType extends UserArgumentType {
    get id(): string {
        return 'user-or-author';
    }

    canBeEmpty(): boolean {
        return true;
    }

    default({ message }: CommandMessageContext): User | null {
        return message.author;
    }
}

export = UserOrAuthorArgumentType;
