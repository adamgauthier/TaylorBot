-- Deploy taylorbot-postgres:0005_update_1.8.0_message_of_the_day to pg

BEGIN;

UPDATE commands.messages_of_the_day SET
    message = 'Use `{prefix}roles` to see what roles you can get in a server!',
    priority_from = NULL,
    priority_to = NULL
WHERE message = 'TaylorBot 1.8.0 is out! Try the new `{prefix}roles` command to see what roles you can get in a server!';

COMMIT;
