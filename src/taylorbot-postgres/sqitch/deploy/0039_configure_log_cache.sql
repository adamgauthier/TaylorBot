-- Deploy taylorbot-postgres:0039_configure_log_cache to pg

BEGIN;

ALTER TABLE plus.plus_guilds
    ADD COLUMN message_content_cache_expiry interval DEFAULT INTERVAL '10 MINUTES' NOT NULL;

UPDATE commands.messages_of_the_day
SET message = 'You can send and receive messages through your moderation team. As a moderator, use </modmail message-user:1085965529413582929> and as a user, use </modmail message-mods:1085965529413582929>!'
WHERE message = 'You can send and receive messages through your moderation team. As a moderator, use </mod mail message-user:838266590294048778> and as a user, use </mod mail message-mods:838266590294048778>!';

UPDATE configuration.application_info SET info_value = '1.16.0' WHERE info_key = 'product_version';

COMMIT;
