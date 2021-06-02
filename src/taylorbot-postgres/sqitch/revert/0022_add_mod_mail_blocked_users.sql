-- Revert taylorbot-postgres:0022_add_mod_mail_blocked_users from pg

BEGIN;

DROP TABLE moderation.mod_mail_blocked_users;

COMMIT;
