-- Revert taylorbot-postgres:0027_edited_log_channels from pg

BEGIN;

DROP TABLE plus.edited_log_channels;

COMMIT;
