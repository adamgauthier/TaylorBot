-- Revert taylorbot-postgres:0023_bump_version_1.12.0 from pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.11.1' WHERE info_key = 'product_version';

DELETE FROM commands.messages_of_the_day WHERE
    message = 'TaylorBot 1.12.0 is out! You can now send and receive messages through your moderation team. As a moderator, use **/mod mail message-user** and as a user, use **/mod mail message-mods**!'
;

COMMIT;
