-- Verify taylorbot-postgres:20241130_coupons on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT NOT EXISTS(SELECT FROM commands.messages_of_the_day WHERE
        position(' and for ' in message) > 0
    ));

    ASSERT (SELECT NOT EXISTS(
        SELECT FROM information_schema.tables
        WHERE table_schema = 'checkers' AND table_name = 'instagram_checker'
    ));

    ASSERT (SELECT EXISTS(
        SELECT FROM information_schema.tables
        WHERE table_schema = 'commands' AND table_name = 'coupons'
    ));

    ASSERT (SELECT EXISTS(
        SELECT FROM information_schema.tables
        WHERE table_schema = 'users' AND table_name = 'redeemed_coupons'
    ));
END $$;

ROLLBACK;
