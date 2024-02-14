-- Verify taylorbot-postgres:20240213_bump_version_1.20.0 on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT info_value FROM configuration.application_info WHERE info_key = 'product_version') = '1.20.0';

    ASSERT (SELECT NOT EXISTS(SELECT FROM commands.messages_of_the_day WHERE
        position('{prefix}roles' in message) > 0
    ));

    ASSERT (SELECT NOT EXISTS(
        SELECT FROM information_schema.tables
        WHERE table_schema = 'guilds' AND table_name = 'guild_role_groups'
    ));
    ASSERT (SELECT NOT EXISTS(
        SELECT FROM information_schema.tables
        WHERE table_schema = 'commands' AND table_name = 'user_groups'
    ));

    ASSERT (SELECT NOT EXISTS(
        SELECT FROM information_schema.columns
        WHERE table_schema = 'commands' AND table_name = 'commands' AND column_name = 'successful_use_count'
    ));
    ASSERT (SELECT NOT EXISTS(
        SELECT FROM information_schema.columns
        WHERE table_schema = 'commands' AND table_name = 'commands' AND column_name = 'unhandled_error_count'
    ));
END $$;

ROLLBACK;
