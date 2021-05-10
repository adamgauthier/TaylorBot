-- Revert taylorbot-postgres:0005_update_1.8.0_message_of_the_day from pg

BEGIN;

UPDATE commands.messages_of_the_day SET
    message = 'TaylorBot 1.8.0 is out! Try the new `{prefix}roles` command to see what roles you can get in a server!',
    priority_from = timestamp with time zone '2020-09-18',
    priority_to = timestamp with time zone '2020-09-20'
WHERE message = 'Use `{prefix}roles` to see what roles you can get in a server!';

COMMIT;
