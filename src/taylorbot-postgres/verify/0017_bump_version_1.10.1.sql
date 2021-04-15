-- Verify taylorbot-postgres:0017_bump_version_1.10.1 on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT info_value FROM configuration.application_info WHERE info_key = 'product_version') = '1.10.1';

    ASSERT (SELECT EXISTS((SELECT FROM commands.messages_of_the_day WHERE
        message = 'TaylorBot 1.10.1 is out! Try the **new Slash Command experience** by typing `/avatar`! No more typos in the command format or forgetting the server prefix!'
    )));
END $$;

ROLLBACK;
