-- Verify taylorbot-postgres:20250614_daily_messages_overrides on pg

BEGIN;

-- Verify id column exists and is UUID type
DO $$
BEGIN
    ASSERT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'commands'
        AND table_name = 'messages_of_the_day'
        AND column_name = 'id'
        AND data_type = 'uuid'
    ), 'id column should exist and be of type uuid';
END $$;

-- Verify primary key is on id column
DO $$
BEGIN
    ASSERT EXISTS (
        SELECT 1
        FROM information_schema.table_constraints tc
        JOIN information_schema.constraint_column_usage ccu
            ON tc.constraint_name = ccu.constraint_name
        WHERE tc.table_schema = 'commands'
        AND tc.table_name = 'messages_of_the_day'
        AND tc.constraint_type = 'PRIMARY KEY'
        AND ccu.column_name = 'id'
    ), 'Primary key should be on id column';
END $$;

ROLLBACK;
