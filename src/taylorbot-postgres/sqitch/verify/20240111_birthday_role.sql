-- Verify taylorbot-postgres:0045_birthday_role on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT EXISTS(
        SELECT FROM information_schema.tables
        WHERE table_schema = 'plus' AND table_name = 'birthday_roles'
    ));

    ASSERT (SELECT EXISTS(
        SELECT FROM information_schema.tables
        WHERE table_schema = 'plus' AND table_name = 'birthday_roles_given'
    ));

    ASSERT (SELECT NOT EXISTS(SELECT FROM commands.messages_of_the_day WHERE
        position('{prefix}gamble' in message) > 0
    ));

    ASSERT (SELECT NOT EXISTS(SELECT FROM commands.messages_of_the_day WHERE
        position('{prefix}rps' in message) > 0
    ));

    ASSERT (SELECT NOT EXISTS(SELECT FROM commands.messages_of_the_day WHERE
        position('{prefix}heist' in message) > 0
    ));
END $$;

ROLLBACK;
