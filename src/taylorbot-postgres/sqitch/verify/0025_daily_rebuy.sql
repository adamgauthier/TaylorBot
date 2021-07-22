-- Verify taylorbot-postgres:0025_daily_rebuy on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT EXISTS(
        SELECT FROM information_schema.columns
        WHERE table_schema = 'users' AND table_name = 'daily_payouts' AND column_name = 'max_streak_count'
    ));
END $$;

ROLLBACK;
