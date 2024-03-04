-- Revert taylorbot-postgres:20240220_leaderboard_perf from pg

BEGIN;

DROP MATERIALIZED VIEW attributes.birthday_calendar_6months;

DROP INDEX guilds.idx_members_taypoints;

ALTER TABLE guilds.guild_members
    DROP COLUMN last_known_taypoint_count;

DROP INDEX guilds.idx_members_minutes;

DROP INDEX guilds.idx_members_messages;

ALTER TABLE guilds.guild_members
    ADD COLUMN minutes_milestone integer DEFAULT 0 NOT NULL;

DROP INDEX guilds.idx_members_last_spoke;

COMMIT;
