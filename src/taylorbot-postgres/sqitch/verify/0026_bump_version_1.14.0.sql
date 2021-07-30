-- Verify taylorbot-postgres:0026_bump_version_1.14.0 on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT info_value FROM configuration.application_info WHERE info_key = 'product_version') = '1.14.0';

    ASSERT (SELECT EXISTS((SELECT FROM commands.messages_of_the_day WHERE
        message = 'TaylorBot 1.14.0 is out! For a limited time, use **/daily claim** to get **more taypoints every day**!'
    )));

    ASSERT (SELECT EXISTS((SELECT FROM commands.messages_of_the_day WHERE
        message = 'TaylorBot 1.14.0 is out! You can now re-buy a lost daily streak with **/daily rebuy**!'
    )));

    ASSERT (SELECT EXISTS((SELECT FROM commands.messages_of_the_day WHERE
        message = 'Ask TaylorBot to remind you about something using **/remind add**!'
    )));

    ASSERT (SELECT EXISTS((SELECT FROM commands.messages_of_the_day WHERE
        message = 'Use **/choose** to have TaylorBot make a decision for you!'
    )));
END $$;

ROLLBACK;
