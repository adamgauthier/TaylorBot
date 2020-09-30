import { Guild, GuildMember } from 'discord.js';

export class DiscordUtil {
    static async getMember(guild: Guild, userId: string): Promise<GuildMember | null> {
        const cached = guild.members.cache.get(userId);

        if (cached != undefined) {
            return cached;
        }
        else {
            try {
                const fetchedMember = await guild.members.fetch(userId);
                return fetchedMember;
            } catch (e) {
                // Not a member
            }
        }

        return null;
    }
}
