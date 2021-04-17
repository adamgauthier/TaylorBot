-- Verify taylorbot-postgres:0018_mod_log_channels on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT EXISTS(
        SELECT FROM information_schema.tables
        WHERE table_schema = 'moderation' AND table_name = 'mod_log_channels'
    ));
END $$;

ROLLBACK;
