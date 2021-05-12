-- Deploy taylorbot-postgres:0020_remove_cleverbot_sessions to pg

BEGIN;

DROP TABLE users.cleverbot_sessions;

COMMIT;
