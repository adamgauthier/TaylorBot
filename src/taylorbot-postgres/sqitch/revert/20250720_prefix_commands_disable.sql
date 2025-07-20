-- Revert taylorbot-postgres:20250720_prefix_commands_disable from pg

BEGIN;

DELETE FROM commands.commands WHERE name = 'all-prefix';

COMMIT;
