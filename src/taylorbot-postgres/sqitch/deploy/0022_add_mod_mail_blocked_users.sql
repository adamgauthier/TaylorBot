-- Deploy taylorbot-postgres:0022_add_mod_mail_blocked_users to pg

BEGIN;

CREATE TABLE moderation.mod_mail_blocked_users (
    guild_id text NOT NULL,
    user_id text NOT NULL,
    added_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (guild_id, user_id)
);

COMMIT;
