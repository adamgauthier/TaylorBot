'use strict';

const MentionedUsersArgumentType = require('./MentionedUsers.js');
const ArgumentParsingError = require('../ArgumentParsingError.js');

class MentionedUsersNotAuthorArgumentType extends MentionedUsersArgumentType {
    get id() {
        return 'mentioned-users-not-author';
    }

    async parse(val, commandContext, info) {
        const users = await super.parse(val, commandContext, info);

        if (users.some(user => user.id === commandContext.message.author.id)) {
            throw new ArgumentParsingError(`You can't mention yourself.`);
        }

        return users;
    }
}

module.exports = MentionedUsersNotAuthorArgumentType;