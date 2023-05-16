-- Verify taylorbot-postgres:0039_configure_log_cache on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT EXISTS(
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'plus' AND table_name = 'plus_guilds' AND column_name = 'message_content_cache_expiry'
    ));

    ASSERT (SELECT NOT EXISTS((SELECT FROM commands.messages_of_the_day WHERE
        position('/mod mail' in message) > 0
    )));

    ASSERT (SELECT info_value FROM configuration.application_info WHERE info_key = 'product_version') = '1.16.0';
END $$;

ROLLBACK;
