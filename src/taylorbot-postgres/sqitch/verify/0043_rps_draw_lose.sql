-- Verify taylorbot-postgres:0043_rps_draw_lose on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT data_type FROM information_schema.columns WHERE table_schema = 'users' AND table_name = 'rps_stats' AND column_name = 'rps_win_count') = 'integer';

    ASSERT (SELECT EXISTS(
        SELECT FROM information_schema.columns
        WHERE table_schema = 'users' AND table_name = 'rps_stats' AND column_name = 'rps_draw_count'
    ));

    ASSERT (SELECT EXISTS(
        SELECT FROM information_schema.columns
        WHERE table_schema = 'users' AND table_name = 'rps_stats' AND column_name = 'rps_lose_count'
    ));
END $$;

ROLLBACK;
