-- Deploy taylorbot-postgres:0009_command_disabled_message to pg

BEGIN;

ALTER TABLE commands.commands
    RENAME enabled TO disabled_message;

ALTER TABLE commands.commands
    ALTER COLUMN disabled_message TYPE text USING CASE WHEN disabled_message = TRUE THEN '' ELSE 'No message.' END;

ALTER TABLE commands.commands
    ALTER COLUMN disabled_message SET DEFAULT '';

DELETE FROM commands.commands WHERE name = 'enablecommand' OR name = 'disablecommand';

COMMIT;
