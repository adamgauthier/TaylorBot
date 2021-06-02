-- Verify taylorbot-postgres:0022_add_mod_mail_blocked_users on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT EXISTS(
        SELECT FROM information_schema.tables
        WHERE table_schema = 'moderation' AND table_name = 'mod_mail_blocked_users'
    ));
END $$;

ROLLBACK;
