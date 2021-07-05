-- Revert taylorbot-postgres:0024_bump_version_1.13.0 from pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.12.0' WHERE info_key = 'product_version';

DELETE FROM commands.messages_of_the_day WHERE
    message = 'TaylorBot 1.13.0 is out! Try the revamped reminder feature! Ask TaylorBot to remind you about something using **/remind add**.'
;

UPDATE commands.messages_of_the_day SET
    message = 'TaylorBot 1.12.0 is out! You can now send and receive messages through your moderation team. As a moderator, use **/mod mail message-user** and as a user, use **/mod mail message-mods**!',
    priority_from = timestamp with time zone '2021-06-06',
    priority_to = timestamp with time zone '2021-06-08'
WHERE message = 'You can send and receive messages through your moderation team. As a moderator, use **/mod mail message-user** and as a user, use **/mod mail message-mods**!';

COMMIT;
