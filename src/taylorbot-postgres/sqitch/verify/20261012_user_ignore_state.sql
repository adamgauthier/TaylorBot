-- Verify taylorbot-postgres:20261012_user_ignore_state on pg

BEGIN;

DO $$
BEGIN
    ASSERT EXISTS (
        SELECT FROM information_schema.columns
        WHERE table_schema = 'users' AND table_name = 'users' AND column_name = 'created_at'
            AND data_type = 'timestamp with time zone' AND is_nullable = 'NO' AND column_default = 'now()'
    );
    ASSERT EXISTS (
        SELECT FROM information_schema.columns
        WHERE table_schema = 'users' AND table_name = 'users' AND column_name = 'ignore_until'
            AND data_type = 'timestamp with time zone' AND is_nullable = 'YES' AND column_default IS NULL
    );
END $$;

ROLLBACK;
