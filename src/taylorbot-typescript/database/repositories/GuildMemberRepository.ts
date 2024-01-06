import { Log } from '../../tools/Logger';
import { Format } from '../../modules/discord/DiscordFormatter';
import * as pgPromise from 'pg-promise';
import { GuildMember } from 'discord.js';

export class GuildMemberRepository {
    readonly #db: pgPromise.IDatabase<unknown>;

    constructor(db: pgPromise.IDatabase<unknown>) {
        this.#db = db;
    }

    mapMemberToDatabase(guildMember: GuildMember): { guild_id: string; user_id: string } {
        return {
            guild_id: guildMember.guild.id,
            user_id: guildMember.id
        };
    }

    async addOrUpdateMemberAsync(guildMember: GuildMember, lastSpokeAt: Date | null): Promise<boolean> {
        const databaseMember = this.mapMemberToDatabase(guildMember);
        try {
            const { experience } = await this.#db.one(
                `INSERT INTO guilds.guild_members (guild_id, user_id, first_joined_at, last_spoke_at) VALUES ($[guild_id], $[user_id], $[first_joined_at], $[last_spoke_at])
                ON CONFLICT (guild_id, user_id) DO UPDATE SET
                    alive = TRUE,
                    first_joined_at = CASE
                        WHEN guild_members.first_joined_at IS NULL AND excluded.first_joined_at IS NOT NULL
                        THEN excluded.first_joined_at
                        ELSE guild_members.first_joined_at
                    END,
                    experience = guild_members.experience + 1,
                    last_spoke_at = CASE
                        WHEN excluded.last_spoke_at IS NULL
                        THEN guild_members.last_spoke_at
                        ELSE excluded.last_spoke_at
                    END
                RETURNING experience;`,
                {
                    first_joined_at: guildMember.joinedAt,
                    last_spoke_at: lastSpokeAt,
                    ...databaseMember
                }
            );

            return experience === '0';
        }
        catch (e) {
            Log.error(`Adding or updating ${Format.member(guildMember)}: ${e}`);
            throw e;
        }
    }
}
