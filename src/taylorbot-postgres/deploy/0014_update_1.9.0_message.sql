-- Deploy taylorbot-postgres:0014_update_1.9.0_message to pg

BEGIN;

UPDATE commands.messages_of_the_day SET
    message = 'Don''t recognize someone? They might have changed their name, use `{prefix}usernames` to see their username history. Use `{prefix}usernames private` to make yours private!',
    priority_from = NULL,
    priority_to = NULL
WHERE message = 'TaylorBot 1.9.0 is out! You can now make your username history private with `{prefix}usernames private`!';

INSERT INTO commands.messages_of_the_day (message) VALUES
('TaylorBot keeps track of how active you are in each server. Use `{prefix}minutes` and `{prefix}rankminutes` to see time spent in a server!');

COMMIT;
