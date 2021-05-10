-- Deploy taylorbot-postgres:0015_plus_schema to pg

BEGIN;

CREATE SCHEMA plus;

ALTER TABLE users.pro_users
    SET SCHEMA plus;

ALTER TABLE plus.pro_users
    RENAME TO plus_users;

ALTER TABLE plus.plus_users
    DROP COLUMN expires_at;

ALTER TABLE plus.plus_users
    RENAME subscription_count TO max_plus_guilds;

ALTER TABLE plus.plus_users
    ADD COLUMN source text DEFAULT 'manual_dont_reward' NOT NULL;

ALTER TABLE plus.plus_users
    ALTER COLUMN source DROP DEFAULT;

ALTER TABLE plus.plus_users
    ADD COLUMN rewarded_for_charge_at text DEFAULT NULL;

ALTER TABLE plus.plus_users
    RENAME CONSTRAINT pro_users_pkey TO plus_users_pkey;

ALTER TABLE plus.plus_users
    ADD COLUMN active boolean DEFAULT TRUE NOT NULL;

ALTER TABLE plus.plus_users
    ALTER COLUMN active DROP DEFAULT;

ALTER TABLE plus.plus_users
    ADD COLUMN metadata text DEFAULT NULL;


ALTER TABLE guilds.pro_guilds
    SET SCHEMA plus;

ALTER TABLE plus.pro_guilds
    RENAME TO plus_guilds;

ALTER TABLE plus.plus_guilds
    RENAME pro_user_id TO plus_user_id;

ALTER TABLE plus.plus_guilds
    ADD COLUMN state TEXT DEFAULT 'enabled' NOT NULL;

ALTER TABLE plus.plus_guilds
    ALTER COLUMN state DROP DEFAULT;

ALTER TABLE plus.plus_guilds
    RENAME CONSTRAINT pro_guilds_pkey TO plus_guilds_pkey;

ALTER TABLE plus.plus_guilds
    RENAME CONSTRAINT pro_user_id_fk TO plus_user_id_fk;


UPDATE commands.commands SET name = 'plus' WHERE name = 'support';

UPDATE commands.messages_of_the_day
SET message = 'TaylorBot is funded by the community, thanks to our TaylorBot Plus members. Learn more with `{prefix}plus`.'
WHERE message = 'TaylorBot is funded by the community, thanks to our Patreon supporters. Learn more with `{prefix}support`.';

CREATE TABLE plus.deleted_log_channels (
    guild_id text NOT NULL,
    deleted_log_channel_id text NOT NULL,
    added_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (guild_id)
);

INSERT INTO plus.deleted_log_channels (guild_id, deleted_log_channel_id)
SELECT guild_id, channel_id FROM guilds.text_channels WHERE is_message_log = TRUE;

ALTER TABLE guilds.text_channels
    DROP COLUMN is_message_log;

CREATE TABLE plus.member_log_channels (
    guild_id text NOT NULL,
    member_log_channel_id text NOT NULL,
    added_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (guild_id)
);

INSERT INTO plus.member_log_channels (guild_id, member_log_channel_id)
SELECT guild_id, channel_id FROM guilds.text_channels WHERE is_member_log = TRUE;

ALTER TABLE guilds.text_channels
    DROP COLUMN is_member_log;

COMMIT;
