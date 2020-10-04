-- Deploy taylorbot-postgres:0006_rename_setprefix_command to pg

BEGIN;

UPDATE commands.commands SET name = 'prefix' WHERE name = 'setprefix';

COMMIT;
