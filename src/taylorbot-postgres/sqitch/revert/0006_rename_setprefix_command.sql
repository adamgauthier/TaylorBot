-- Revert taylorbot-postgres:0006_rename_setprefix_command from pg

BEGIN;

UPDATE commands.commands SET name = 'setprefix' WHERE name = 'prefix';

COMMIT;
