-- Verify taylorbot-postgres:0008_bump_version_1.8.1 on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT info_value FROM configuration.application_info WHERE info_key = 'product_version') = '1.8.1';

    ASSERT (SELECT EXISTS((SELECT 1 FROM commands.messages_of_the_day WHERE
        message = 'Do you like TaylorBot? Do you want to add it to another server you''re in? Go to https://taylorbot.app/ to get started!'
    )));
END $$;

ROLLBACK;
