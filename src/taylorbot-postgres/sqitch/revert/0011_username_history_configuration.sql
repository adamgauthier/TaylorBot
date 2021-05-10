-- Revert taylorbot-postgres:0011_username_history_configuration from pg

BEGIN;

DROP TABLE users.username_history_configuration;

COMMIT;
