-- Revert taylorbot-postgres:0009_command_disabled_message from pg

BEGIN;

ALTER TABLE commands.commands
    RENAME disabled_message TO enabled;

ALTER TABLE commands.commands
    ALTER COLUMN enabled DROP DEFAULT;

ALTER TABLE commands.commands
    ALTER COLUMN enabled TYPE boolean USING CASE WHEN enabled = '' THEN TRUE ELSE FALSE END;

ALTER TABLE commands.commands
    ALTER COLUMN enabled SET DEFAULT TRUE;

COMMIT;
