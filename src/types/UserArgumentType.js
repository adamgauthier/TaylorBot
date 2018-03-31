'use strict';

const { GlobalPaths } = require('globalobjects');

const ArgumentType = require(GlobalPaths.ArgumentType);

class UserArgumentType extends ArgumentType {
    constructor(taylorbot) {
        super(taylorbot, 'user');
    }

    validate(val, message, arg) {
        // TODO: actually validate
        return true;
    }

    parse(val, message, arg) {
        const matches = val.match(/^(?:<@!?)?([0-9]+)>?$/);
        if (matches) {
            const user = this.taylorbot.resolveUser(matches[1]);
            if (user)
                return user;
        }

        const { guild } = message;
        if (guild && guild.available) {
            const search = val.toLowerCase();
            const members = guild.members.filterArray(memberFilterInexact(search));

            if (members.length === 1)
                return members[0].user;
            if (members.length > 1) {
                const exactMembers = members.filter(memberFilterExact(search));
                if (exactMembers.length === 1)
                    return exactMembers[0].user;
            }
        }

        return message.author;
    }
}

const memberFilterExact = search => {
    return mem => mem.user.username.toLowerCase() === search ||
        (mem.nickname && mem.nickname.toLowerCase() === search) ||
        `${mem.user.username.toLowerCase()}#${mem.user.discriminator}` === search;
};

const memberFilterInexact = search => {
    return mem => mem.user.username.toLowerCase().includes(search) ||
        (mem.nickname && mem.nickname.toLowerCase().includes(search)) ||
        `${mem.user.username.toLowerCase()}#${mem.user.discriminator}`.includes(search);
};

module.exports = UserArgumentType;