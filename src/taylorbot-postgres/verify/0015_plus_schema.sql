-- Verify taylorbot-postgres:0015_plus_schema on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT EXISTS(
        SELECT FROM information_schema.tables
        WHERE table_schema = 'plus' AND table_name = 'plus_users'
    ));
    ASSERT (SELECT EXISTS(
        SELECT FROM information_schema.tables
        WHERE table_schema = 'plus' AND table_name = 'plus_guilds'
    ));
    ASSERT (SELECT NOT EXISTS(
        SELECT FROM information_schema.tables
        WHERE table_schema = 'users' AND table_name = 'pro_users'
    ));
    ASSERT (SELECT NOT EXISTS(
        SELECT FROM information_schema.tables
        WHERE table_schema = 'guilds' AND table_name = 'pro_guilds'
    ));
    ASSERT (SELECT EXISTS((SELECT FROM commands.messages_of_the_day WHERE
        message = 'TaylorBot is funded by the community, thanks to our TaylorBot Plus members. Learn more with `{prefix}plus`.'
    )));
    ASSERT (SELECT EXISTS(
        SELECT FROM information_schema.tables
        WHERE table_schema = 'plus' AND table_name = 'deleted_log_channels'
    ));
    ASSERT (SELECT EXISTS(
        SELECT FROM information_schema.tables
        WHERE table_schema = 'plus' AND table_name = 'member_log_channels'
    ));
END $$;

ROLLBACK;
