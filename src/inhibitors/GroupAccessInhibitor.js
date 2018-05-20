'use strict';

const { Paths } = require('globalobjects');

const Inhibitor = require(Paths.Inhibitor);
const Log = require(Paths.Logger);
const Format = require(Paths.DiscordFormatter);
const { masterId } = require(Paths.TaylorBotConfig);
const UserGroups = require(Paths.UserGroups);

class GroupAccessInhibitor extends Inhibitor {
    shouldBeBlocked(message, command) {
        const { author, member, client } = message;

        if (!member)
            return false;

        const { oldRegistry } = client;

        if (!GroupAccessInhibitor.groupHasAccess(member, command.command.minimumGroup.accessLevel, oldRegistry.guilds, oldRegistry.groups)) {
            Log.verbose(`Command '${command.name}' can't be used by ${Format.user(author)} because they don't have the minimum group '${command.command.minimumGroup.name}'.`);
            return true;
        }

        return false;
    }

    static groupHasAccess(member, minimumGroupLevel, guilds, groups) {
        let { accessLevel } = member.id === masterId ? UserGroups.Master : UserGroups.Everyone;
        if (accessLevel >= minimumGroupLevel)
            return true;

        const guildRoles = guilds.get(member.guild.id).roleGroups;
        const ownedGroups = member.roles.map(role => guildRoles[role.id]).filter(g => g);

        for (const group of ownedGroups) {
            accessLevel = groups.get(group).accessLevel;
            if (accessLevel >= minimumGroupLevel)
                return true;
        }

        return false;
    }
}

module.exports = GroupAccessInhibitor;