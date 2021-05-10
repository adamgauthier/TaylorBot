-- Verify taylorbot-postgres:0006_rename_setprefix_command on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT NOT EXISTS(
        (SELECT 1 FROM commands.commands WHERE name = 'setprefix')
    ));
END $$;

ROLLBACK;
