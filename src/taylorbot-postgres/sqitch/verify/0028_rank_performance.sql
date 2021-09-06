-- Verify taylorbot-postgres:0028_rank_performance on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT EXISTS(
        SELECT FROM pg_indexes
        WHERE schemaname = 'guilds' AND tablename = 'guild_members' AND indexname = 'idx_members_first_joined_at'
    ));
END $$;

ROLLBACK;
