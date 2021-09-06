-- Deploy taylorbot-postgres:0028_rank_performance to pg

BEGIN;

CREATE INDEX idx_members_first_joined_at ON guilds.guild_members(guild_id, first_joined_at ASC);

COMMIT;
