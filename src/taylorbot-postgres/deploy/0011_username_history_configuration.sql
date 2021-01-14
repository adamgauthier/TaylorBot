-- Deploy taylorbot-postgres:0011_username_history_configuration to pg

BEGIN;

CREATE TABLE users.username_history_configuration (
    user_id text NOT NULL,
    last_changed_at timestamp with time zone DEFAULT now() NOT NULL,
    is_hidden boolean NOT NULL,
    PRIMARY KEY (user_id),
    CONSTRAINT user_id_fk FOREIGN KEY (user_id) REFERENCES users.users(user_id)
);

COMMIT;
