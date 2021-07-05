-- Verify taylorbot-postgres:0024_bump_version_1.13.0 on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT info_value FROM configuration.application_info WHERE info_key = 'product_version') = '1.13.0';

    ASSERT (SELECT EXISTS((SELECT FROM commands.messages_of_the_day WHERE
        message = 'TaylorBot 1.13.0 is out! Try the revamped reminder feature! Ask TaylorBot to remind you about something using **/remind add**.'
    )));
END $$;

ROLLBACK;
