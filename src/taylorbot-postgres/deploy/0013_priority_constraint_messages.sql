-- Deploy taylorbot-postgres:0013_priority_constraints_messages to pg

BEGIN;

UPDATE commands.messages_of_the_day SET
    priority_to = timestamp with time zone '2021-01-21'
WHERE message = 'TaylorBot 1.9.0 is out! You can now make your username history private with `{prefix}usernames private`!';

ALTER TABLE commands.messages_of_the_day
    ADD CONSTRAINT check_date_range CHECK (
        (priority_from IS NULL AND priority_to IS NULL) OR
        (priority_from IS NOT NULL AND priority_to IS NOT NULL AND priority_from < priority_to)
    );

COMMIT;
