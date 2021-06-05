-- Deploy taylorbot-postgres:0023_bump_version_1.12.0 to pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.12.0' WHERE info_key = 'product_version';

INSERT INTO commands.messages_of_the_day (message, priority_from, priority_to) VALUES
('TaylorBot 1.12.0 is out! You can now send and receive messages through your moderation team. As a moderator, use **/mod mail message-user** and as a user, use **/mod mail message-mods**!', timestamp with time zone '2021-06-06', timestamp with time zone '2021-06-08');

COMMIT;
