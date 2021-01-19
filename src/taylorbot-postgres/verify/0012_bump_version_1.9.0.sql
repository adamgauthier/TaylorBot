-- Verify taylorbot-postgres:0012_bump_version_1.9.0 on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT info_value FROM configuration.application_info WHERE info_key = 'product_version') = '1.9.0';

    ASSERT (SELECT EXISTS((SELECT FROM commands.messages_of_the_day WHERE
        message = 'TaylorBot 1.9.0 is out! You can now make your username history private with `{prefix}usernames private`!'
    )));
END $$;

ROLLBACK;
