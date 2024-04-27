-- Revert taylorbot-postgres:20240426_duplicate_commands from pg

BEGIN;

UPDATE commands.messages_of_the_day
SET message = 'Don''t recognize someone? They might have changed their name, use `{prefix}usernames` to see their username history. Use `{prefix}usernames private` to make yours private!'
WHERE message = 'Don''t recognize someone? Use </usernames show:1214478089883619378> to check if they changed their username! Use </usernames visibility:1214478089883619378> to make your history private! 🔍';

COMMIT;
