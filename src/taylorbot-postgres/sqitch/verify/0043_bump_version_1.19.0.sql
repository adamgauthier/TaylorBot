-- Verify taylorbot-postgres:0043_bump_version_1.19.0 on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT NOT EXISTS(SELECT FROM commands.messages_of_the_day WHERE
        position('{prefix}minutes' in message) > 0
    ));

    ASSERT (SELECT NOT EXISTS(SELECT FROM commands.messages_of_the_day WHERE
        position('{prefix}joined' in message) > 0
    ));

    ASSERT (SELECT info_value FROM configuration.application_info WHERE info_key = 'product_version') = '1.19.0';
END $$;

ROLLBACK;
