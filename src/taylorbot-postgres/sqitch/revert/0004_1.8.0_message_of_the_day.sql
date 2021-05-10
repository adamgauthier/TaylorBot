-- Revert taylorbot-postgres:0004_1.8.0_message_of_the_day from pg

BEGIN;

DELETE FROM commands.messages_of_the_day WHERE
    message = 'TaylorBot 1.8.0 is out! Try the new `{prefix}roles` command to see what roles you can get in a server!'
;

COMMIT;
