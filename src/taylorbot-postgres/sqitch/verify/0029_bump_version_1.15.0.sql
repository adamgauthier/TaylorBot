-- Verify taylorbot-postgres:0029_bump_version_1.15.0 on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT info_value FROM configuration.application_info WHERE info_key = 'product_version') = '1.15.0';

    ASSERT (SELECT EXISTS((SELECT FROM commands.messages_of_the_day WHERE
        message = 'TaylorBot 1.15.0 is out! Threads are now fully supported!'
    )));

    ASSERT (SELECT EXISTS((SELECT FROM commands.messages_of_the_day WHERE
        message = 'TaylorBot 1.15.0 is out! If you have a **TaylorBot Plus** server, you can monitor deleted and edited messages. Use **/monitor deleted set** and **/monitor edited set** to get started!'
    )));

    ASSERT (SELECT EXISTS((SELECT FROM commands.messages_of_the_day WHERE
        message = 'For a limited time, use **/daily claim** to get **more taypoints every day**!'
    )));

    ASSERT (SELECT EXISTS((SELECT FROM commands.messages_of_the_day WHERE
        message = 'Lost a longtime daily streak? Re-buy it with **/daily rebuy**!'
    )));
END $$;

ROLLBACK;
