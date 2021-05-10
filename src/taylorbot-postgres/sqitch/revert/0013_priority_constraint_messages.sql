-- Revert taylorbot-postgres:0013_priority_constraints_messages from pg

BEGIN;

ALTER TABLE commands.messages_of_the_day DROP CONSTRAINT check_date_range;

UPDATE commands.messages_of_the_day SET
    priority_to = timestamp with time zone '2020-01-20'
WHERE message = 'TaylorBot 1.9.0 is out! You can now make your username history private with `{prefix}usernames private`!';

COMMIT;
