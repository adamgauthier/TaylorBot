-- Deploy taylorbot-postgres:0027_edited_log_channels to pg

BEGIN;

CREATE TABLE plus.edited_log_channels (
    guild_id text NOT NULL,
    edited_log_channel_id text NOT NULL,
    added_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (guild_id)
);

COMMIT;
