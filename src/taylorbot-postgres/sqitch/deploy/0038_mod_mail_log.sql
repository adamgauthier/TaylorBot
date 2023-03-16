-- Deploy taylorbot-postgres:0038_mod_mail_log to pg

BEGIN;

CREATE TABLE moderation.mod_mail_log_channels (
    guild_id text NOT NULL,
    channel_id text NOT NULL,
    added_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (guild_id)
);

COMMIT;
