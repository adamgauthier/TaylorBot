-- Revert taylorbot-postgres:0018_mod_log_channels from pg

BEGIN;

DROP TABLE moderation.mod_log_channels;

DROP SCHEMA moderation;

COMMIT;
