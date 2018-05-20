'use strict';

const { Paths } = require('globalobjects');

const ArgumentType = require(Paths.ArgumentType);

const MemberArgumentType = require('./MemberArgumentType');

class MemberOrAuthorArgumentType extends ArgumentType {
    constructor() {
        super('member-or-author');
        // TODO: Get it from registry?
        this.memberArgumentType = new MemberArgumentType();
    }

    isEmpty(val, message) {
        return message.member ? false : !val;
    }

    parse(val, message, arg) {
        if (!val) {
            return message.member;
        }

        return this.memberArgumentType.parse(val, message, arg);
    }
}

module.exports = MemberOrAuthorArgumentType;
