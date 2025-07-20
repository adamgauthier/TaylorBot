-- Verify taylorbot-postgres:20250720_prefix_commands_disable on pg

BEGIN;

-- XXX Add verifications here.
DO $$
BEGIN
    ASSERT EXISTS (
        SELECT 1
        FROM commands.commands
        WHERE name = 'all-prefix'
    ), 'all-prefix command should exist in commands.commands';
END $$;

ROLLBACK;
