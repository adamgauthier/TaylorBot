-- Verify taylorbot-postgres:0009_command_disabled_message on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT NOT EXISTS(
        (SELECT 1 FROM commands.commands WHERE name = 'enablecommand' OR name = 'disablecommand')
    ));
    ASSERT (SELECT NOT EXISTS(
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'commands' AND table_name = 'commands' AND column_name = 'enabled'
    ));
    ASSERT (SELECT EXISTS(
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'commands' AND table_name = 'commands' AND column_name = 'disabled_message'
    ));
END $$;

ROLLBACK;
