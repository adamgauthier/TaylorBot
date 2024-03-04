-- Verify taylorbot-postgres:20240220_leaderboard_perf on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT EXISTS(
        SELECT FROM pg_indexes
        WHERE schemaname = 'guilds' AND tablename = 'guild_members' AND indexname = 'idx_members_last_spoke'
    ));
    ASSERT (SELECT NOT EXISTS(
        SELECT FROM information_schema.columns
        WHERE table_schema = 'guilds' AND table_name = 'guild_members' AND column_name = 'minutes_milestone'
    ));
    ASSERT (SELECT EXISTS(
        SELECT FROM pg_indexes
        WHERE schemaname = 'guilds' AND tablename = 'guild_members' AND indexname = 'idx_members_messages'
    ));
    ASSERT (SELECT EXISTS(
        SELECT FROM pg_indexes
        WHERE schemaname = 'guilds' AND tablename = 'guild_members' AND indexname = 'idx_members_minutes'
    ));
    ASSERT (SELECT EXISTS(
        SELECT FROM information_schema.columns
        WHERE table_schema = 'guilds' AND table_name = 'guild_members' AND column_name = 'last_known_taypoint_count'
    ));
    ASSERT (SELECT EXISTS(
        SELECT FROM pg_indexes
        WHERE schemaname = 'guilds' AND tablename = 'guild_members' AND indexname = 'idx_members_taypoints'
    ));
    ASSERT (SELECT EXISTS(
        SELECT FROM pg_indexes
        WHERE schemaname = 'attributes' AND tablename = 'birthday_calendar_6months' AND indexname = 'idx_calendar_user_ids'
    ));
END $$;

ROLLBACK;
