'use strict';

const ArgumentType = require('../structures/ArgumentType.js');
const ArgumentParsingError = require('../structures/ArgumentParsingError.js');

class RoleArgumentType extends ArgumentType {
    constructor() {
        super({
            includesSpaces: true
        });
    }

    get id() {
        return 'role';
    }

    parse(val, { message }) {
        const { guild } = message;

        if (guild) {
            const matches = val.trim().match(/^(?:<@&)?([0-9]+)>?$/);
            if (matches) {
                const role = guild.roles.get(matches[1]);
                if (role) {
                    return role;
                }
            }

            const search = val.toLowerCase();
            const inexactRoles = guild.roles.filter(RoleArgumentType.roleFilterInexact(search));
            if (inexactRoles.size === 0) {
                throw new ArgumentParsingError(`Could not find role '${val}'.`);
            }
            else if (inexactRoles.size === 1) {
                return inexactRoles.first();
            }

            const exactRoles = inexactRoles.filter(RoleArgumentType.roleFilterExact(search));

            return exactRoles.size > 0 ? exactRoles.first() : inexactRoles.first();
        }

        throw new ArgumentParsingError(`Can't find role '${val}' outside of a server.`);
    }

    static roleFilterExact(search) {
        return role => role.name.toLowerCase() === search;
    }

    static roleFilterInexact(search) {
        return role => role.name.toLowerCase().includes(search);
    }
}

module.exports = RoleArgumentType;
