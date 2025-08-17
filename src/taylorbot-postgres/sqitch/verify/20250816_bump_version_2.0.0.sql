-- Verify taylorbot-postgres:20250816_bump_version_2.0.0 on pg

BEGIN;

DO $$
BEGIN
    -- Verify version
    ASSERT (SELECT info_value FROM configuration.application_info WHERE info_key = 'product_version') = '2.0.0';

    -- Verify table has been renamed from gamble_stats to risk_stats
    ASSERT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'users' AND table_name = 'risk_stats');
    ASSERT NOT EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'users' AND table_name = 'gamble_stats');

    -- Verify columns have been renamed
    ASSERT EXISTS (SELECT FROM information_schema.columns WHERE table_schema = 'users' AND table_name = 'risk_stats' AND column_name = 'risk_win_count');
    ASSERT EXISTS (SELECT FROM information_schema.columns WHERE table_schema = 'users' AND table_name = 'risk_stats' AND column_name = 'risk_win_amount');
    ASSERT EXISTS (SELECT FROM information_schema.columns WHERE table_schema = 'users' AND table_name = 'risk_stats' AND column_name = 'risk_lose_count');
    ASSERT EXISTS (SELECT FROM information_schema.columns WHERE table_schema = 'users' AND table_name = 'risk_stats' AND column_name = 'risk_lose_amount');

    ASSERT NOT EXISTS (SELECT FROM information_schema.columns WHERE table_schema = 'users' AND table_name = 'risk_stats' AND column_name = 'gamble_win_count');
    ASSERT NOT EXISTS (SELECT FROM information_schema.columns WHERE table_schema = 'users' AND table_name = 'risk_stats' AND column_name = 'gamble_win_amount');
    ASSERT NOT EXISTS (SELECT FROM information_schema.columns WHERE table_schema = 'users' AND table_name = 'risk_stats' AND column_name = 'gamble_lose_count');
    ASSERT NOT EXISTS (SELECT FROM information_schema.columns WHERE table_schema = 'users' AND table_name = 'risk_stats' AND column_name = 'gamble_lose_amount');

    -- Verify module_name column has been removed from commands.commands table
    ASSERT NOT EXISTS (SELECT FROM information_schema.columns WHERE table_schema = 'commands' AND table_name = 'commands' AND column_name = 'module_name');
END $$;

ROLLBACK;
