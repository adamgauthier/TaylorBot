'use strict';

const { Paths } = require('globalobjects');

const ArgumentType = require(Paths.ArgumentType);
const ArgumentParsingError = require(Paths.ArgumentParsingError);

class RoleArgumentType extends ArgumentType {
    constructor() {
        super('role');
    }

    parse(val, { message }) {
        const { guild } = message;

        if (guild) {
            const matches = val.match(/^(?:<@&)?([0-9]+)>?$/);
            if (matches) {
                const role = guild.roles.get(matches[1]);
                if (role) {
                    return role;
                }
            }

            const search = val.toLowerCase();
            const inexactRoles = guild.roles.filterArray(RoleArgumentType.roleFilterInexact(search));
            if (inexactRoles.length === 0) {
                throw new ArgumentParsingError(`Could not find role '${val}'.`);
            }
            else if (inexactRoles.length === 1) {
                return inexactRoles[0];
            }

            const exactRoles = inexactRoles.filter(RoleArgumentType.roleFilterExact(search));

            return exactRoles.length > 0 ? exactRoles[0] : inexactRoles[0];
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
