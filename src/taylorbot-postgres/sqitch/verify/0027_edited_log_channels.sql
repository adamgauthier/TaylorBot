-- Verify taylorbot-postgres:0027_edited_log_channels on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT EXISTS(
        SELECT FROM information_schema.tables
        WHERE table_schema = 'plus' AND table_name = 'edited_log_channels'
    ));
END $$;

ROLLBACK;
