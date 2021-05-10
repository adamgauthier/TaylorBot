-- Revert taylorbot-postgres:0014_update_1.9.0_message from pg

BEGIN;

UPDATE commands.messages_of_the_day SET
    message = 'TaylorBot 1.9.0 is out! You can now make your username history private with `{prefix}usernames private`!',
    priority_from = timestamp with time zone '2021-01-18',
    priority_to = timestamp with time zone '2021-01-21'
WHERE message = 'Don''t recognize someone? They might have changed their name, use `{prefix}usernames` to see their username history. Use `{prefix}usernames private` to make yours private!';

DELETE FROM commands.messages_of_the_day WHERE
    message = 'TaylorBot keeps track of how active you are in each server. Use `{prefix}minutes` and `{prefix}rankminutes` to see time spent in a server!'
;

COMMIT;
