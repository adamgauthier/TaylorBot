-- Verify taylorbot-postgres:0021_bump_version_1.11.1 on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT info_value FROM configuration.application_info WHERE info_key = 'product_version') = '1.11.1';

    ASSERT (SELECT EXISTS((SELECT FROM commands.messages_of_the_day WHERE
        message = 'Use **/mod log set** to set up a channel to record moderation command usage!' AND
        priority_from IS NULL AND priority_to IS NULL
    )));
END $$;

ROLLBACK;
