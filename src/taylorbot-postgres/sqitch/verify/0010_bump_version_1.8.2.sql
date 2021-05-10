-- Verify taylorbot-postgres:0010_bump_version_1.8.2 on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT info_value FROM configuration.application_info WHERE info_key = 'product_version') = '1.8.2';

    ASSERT (SELECT NOT EXISTS((SELECT 1 FROM commands.messages_of_the_day WHERE
        message = 'Use `{prefix}status` to show off your Discord status, including your currently playing song on Spotify!'
    )));
END $$;

ROLLBACK;
