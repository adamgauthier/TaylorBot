-- Verify taylorbot-postgres:0038_mod_mail_log on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT EXISTS(
        SELECT FROM information_schema.tables
        WHERE table_schema = 'moderation' AND table_name = 'mod_mail_log_channels'
    ));
END $$;

ROLLBACK;
