import { Guild, GuildMember } from 'discord.js';

export class DiscordUtil {
    static async getMember(guild: Guild, userId: string): Promise<GuildMember | null> {
        if (guild.members.has(userId)) {
            return guild.members.get(userId)!;
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
