-- Deploy taylorbot-postgres:0018_mod_log_channels to pg

BEGIN;

CREATE SCHEMA moderation;

CREATE TABLE moderation.mod_log_channels (
    guild_id text NOT NULL,
    channel_id text NOT NULL,
    added_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (guild_id)
);

COMMIT;
