-- Verify taylorbot-postgres:0011_username_history_configuration on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT EXISTS(
        SELECT FROM information_schema.tables
        WHERE table_schema = 'users' AND table_name = 'username_history_configuration'
    ));
END $$;

ROLLBACK;
