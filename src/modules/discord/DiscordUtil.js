'use strict';

class DiscordUtil {
    static async getMember(guild, userId) {
        if (guild.members.has(userId)) {
            return guild.members.get(userId);
        }
        else {
            try {
                const fetchedMember = await guild.members.fetch(userId);
                return fetchedMember;
            } catch (e) {
                //Not a member
            }
        }

        return null;
    }
}

module.exports = DiscordUtil;