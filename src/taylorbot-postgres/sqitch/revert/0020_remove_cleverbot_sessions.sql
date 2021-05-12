-- Revert taylorbot-postgres:0020_remove_cleverbot_sessions from pg

BEGIN;

CREATE TABLE users.cleverbot_sessions (
    user_id text NOT NULL,
    session_created_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (user_id),
    CONSTRAINT user_id_fk FOREIGN KEY (user_id) REFERENCES users.users(user_id)
);

COMMIT;
