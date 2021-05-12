-- Verify taylorbot-postgres:0020_remove_cleverbot_sessions on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT NOT EXISTS(
        SELECT FROM information_schema.tables
        WHERE table_schema = 'users' AND table_name = 'cleverbot_sessions'
    ));
END $$;

ROLLBACK;
