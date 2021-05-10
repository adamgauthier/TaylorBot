-- Revert taylorbot-postgres:0015_plus_schema from pg

BEGIN;

ALTER TABLE guilds.text_channels
    ADD COLUMN is_member_log boolean DEFAULT FALSE NOT NULL;

UPDATE guilds.text_channels SET is_member_log = TRUE
WHERE channel_id IN (SELECT member_log_channel_id FROM plus.member_log_channels);

DROP TABLE plus.member_log_channels;

ALTER TABLE guilds.text_channels
    ADD COLUMN is_message_log boolean DEFAULT FALSE NOT NULL;

UPDATE guilds.text_channels SET is_message_log = TRUE
WHERE channel_id IN (SELECT deleted_log_channel_id FROM plus.deleted_log_channels);

DROP TABLE plus.deleted_log_channels;

UPDATE commands.messages_of_the_day
SET message = 'TaylorBot is funded by the community, thanks to our Patreon supporters. Learn more with `{prefix}support`.'
WHERE message = 'TaylorBot is funded by the community, thanks to our TaylorBot Plus members. Learn more with `{prefix}plus`.';

UPDATE commands.commands SET name = 'support' WHERE name = 'plus';


ALTER TABLE plus.plus_guilds
    RENAME CONSTRAINT plus_user_id_fk TO pro_user_id_fk;

ALTER TABLE plus.plus_guilds
    RENAME CONSTRAINT plus_guilds_pkey TO pro_guilds_pkey;

ALTER TABLE plus.plus_guilds
    DROP COLUMN state;

ALTER TABLE plus.plus_guilds
    RENAME plus_user_id TO pro_user_id;

ALTER TABLE plus.plus_guilds
    RENAME TO pro_guilds;

ALTER TABLE plus.pro_guilds
    SET SCHEMA guilds;


ALTER TABLE plus.plus_users
    DROP COLUMN metadata;

ALTER TABLE plus.plus_users
    DROP COLUMN active;

ALTER TABLE plus.plus_users
    RENAME CONSTRAINT plus_users_pkey TO pro_users_pkey;

ALTER TABLE plus.plus_users
    DROP COLUMN rewarded_for_charge_at;

ALTER TABLE plus.plus_users
    DROP COLUMN source;

ALTER TABLE plus.plus_users
    RENAME max_plus_guilds TO subscription_count;

ALTER TABLE plus.plus_users
    ADD COLUMN expires_at timestamp with time zone DEFAULT NULL;

ALTER TABLE plus.plus_users
    ALTER COLUMN expires_at DROP DEFAULT;

ALTER TABLE plus.plus_users
    RENAME TO pro_users;

ALTER TABLE plus.pro_users
    SET SCHEMA users;

DROP SCHEMA plus;

COMMIT;
