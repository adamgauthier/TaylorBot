-- Verify taylorbot-postgres:0014_update_1.9.0_message on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT EXISTS((SELECT FROM commands.messages_of_the_day WHERE
        message = 'Don''t recognize someone? They might have changed their name, use `{prefix}usernames` to see their username history. Use `{prefix}usernames private` to make yours private!' AND
        priority_from IS NULL AND priority_to IS NULL
    )));
    ASSERT (SELECT EXISTS((SELECT FROM commands.messages_of_the_day WHERE
        message = 'TaylorBot keeps track of how active you are in each server. Use `{prefix}minutes` and `{prefix}rankminutes` to see time spent in a server!'
    )));
END $$;

ROLLBACK;
