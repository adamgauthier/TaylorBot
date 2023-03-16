-- Revert taylorbot-postgres:0038_mod_mail_log from pg

BEGIN;

DROP TABLE moderation.mod_mail_log_channels;

COMMIT;
