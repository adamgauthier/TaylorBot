-- Verify taylorbot-postgres:0041_bump_version_1.18.0 on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT NOT EXISTS(SELECT FROM commands.messages_of_the_day WHERE
        position('{prefix}time' in message) > 0
    ));

    ASSERT (SELECT NOT EXISTS(SELECT FROM commands.messages_of_the_day WHERE
        position('{prefix}gift' in message) > 0
    ));

    ASSERT (SELECT EXISTS(SELECT FROM commands.messages_of_the_day WHERE
        message = 'Use </location weather:1141925890448691270> to see the current weather where you or your friend are!'
    ));

    ASSERT (SELECT info_value FROM configuration.application_info WHERE info_key = 'product_version') = '1.18.0';
END $$;

ROLLBACK;
