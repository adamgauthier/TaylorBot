-- Verify taylorbot-postgres:0019_bump_version_1.11.0 on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT info_value FROM configuration.application_info WHERE info_key = 'product_version') = '1.11.0';

    ASSERT (SELECT EXISTS((SELECT FROM commands.messages_of_the_day WHERE
        message = 'Try the **new Slash Command experience** by typing `/avatar`! No more typos in the command format or forgetting the server prefix!' AND
        priority_from IS NULL AND priority_to IS NULL
    )));

    ASSERT (SELECT EXISTS((SELECT FROM commands.messages_of_the_day WHERE
        message = 'TaylorBot 1.11.0 is out! Use **/mod log set** to set up a channel to record moderator usage of **/kick** and `{prefix}jail`!'
    )));
END $$;

ROLLBACK;
