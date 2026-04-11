-- Verify taylorbot-postgres:20260409_attributes_set_at on pg

BEGIN;

DO $$
BEGIN
    -- Verify parent attributes table has been removed
    ASSERT NOT EXISTS (
        SELECT FROM information_schema.tables
        WHERE table_schema = 'attributes' AND table_name = 'attributes'
    );

    -- Verify set_at column exists on all attribute tables
    ASSERT EXISTS (
        SELECT FROM information_schema.columns
        WHERE table_schema = 'attributes' AND table_name = 'text_attributes' AND column_name = 'set_at'
    );
    ASSERT EXISTS (
        SELECT FROM information_schema.columns
        WHERE table_schema = 'attributes' AND table_name = 'integer_attributes' AND column_name = 'set_at'
    );
    ASSERT EXISTS (
        SELECT FROM information_schema.columns
        WHERE table_schema = 'attributes' AND table_name = 'location_attributes' AND column_name = 'set_at'
    );
    ASSERT EXISTS (
        SELECT FROM information_schema.columns
        WHERE table_schema = 'attributes' AND table_name = 'birthdays' AND column_name = 'set_at'
    );
END $$;

ROLLBACK;
