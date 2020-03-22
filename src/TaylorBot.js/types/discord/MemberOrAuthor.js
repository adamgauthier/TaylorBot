'use strict';

const MemberArgumentType = require('./Member.js');

class MemberOrAuthorArgumentType extends MemberArgumentType {
    get id() {
        return 'member-or-author';
    }

    canBeEmpty({ message }) {
        return message.member ? true : false;
    }

    default({ message }) {
        return message.member;
    }
}

module.exports = MemberOrAuthorArgumentType;
