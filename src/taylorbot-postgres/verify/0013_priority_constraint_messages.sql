-- Verify taylorbot-postgres:0013_priority_constraints_messages on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT priority_to FROM commands.messages_of_the_day WHERE
        message = 'TaylorBot 1.9.0 is out! You can now make your username history private with `{prefix}usernames private`!'
    ) = timestamp with time zone '2021-01-21';

    ASSERT (SELECT EXISTS(
        SELECT FROM information_schema.constraint_column_usage
        WHERE table_schema = 'commands' AND table_name = 'messages_of_the_day' AND constraint_name = 'check_date_range'
    ));
END $$;

ROLLBACK;
