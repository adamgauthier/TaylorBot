'use strict';

const TextArgumentType = require('../base/Text.js');
const MentionedUserArgumentType = require('./MentionedUser.js');

class MentionedUsersArgumentType extends TextArgumentType {
    constructor() {
        super();
        this.mentionedUserArgumentType = new MentionedUserArgumentType();
    }

    get id() {
        return 'mentioned-users';
    }

    parse(val, commandContext, info) {
        const mentions = val.split(' ').filter(mention => mention !== '');

        return Promise.all(
            mentions.map(mention => this.mentionedUserArgumentType.parse(mention, commandContext, info))
        );
    }
}

module.exports = MentionedUsersArgumentType;