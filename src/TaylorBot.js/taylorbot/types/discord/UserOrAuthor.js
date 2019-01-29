'use strict';

const UserArgumentType = require('./User.js');

class UserOrAuthorArgumentType extends UserArgumentType {
    get id() {
        return 'user-or-author';
    }

    canBeEmpty() {
        return true;
    }

    default({ message }) {
        return message.author;
    }
}

module.exports = UserOrAuthorArgumentType;
