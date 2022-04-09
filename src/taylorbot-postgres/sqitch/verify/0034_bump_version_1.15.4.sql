-- Verify taylorbot-postgres:0033_bump_version_1.15.4 on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT info_value FROM configuration.application_info WHERE info_key = 'product_version') = '1.15.4';

    ASSERT (SELECT EXISTS((SELECT FROM commands.messages_of_the_day WHERE
        message = 'Do you like TaylorBot? Do you want to add me to another server you''re in? Use the ''**Add to Server**'' button on my profile to get started!'
    )));
END $$;

ROLLBACK;
