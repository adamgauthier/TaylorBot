-- Deploy taylorbot-postgres:20260409_attributes_set_at to pg

BEGIN;

-- Drop FK constraints referencing the parent attributes table
ALTER TABLE attributes.text_attributes DROP CONSTRAINT attribute_id_fk;
ALTER TABLE attributes.integer_attributes DROP CONSTRAINT attribute_id_fk;

-- Drop the now-unused parent attributes table
DROP TABLE attributes.attributes;

-- Add set_at column to all attribute tables (existing rows get '-infinity' as sentinel)
ALTER TABLE attributes.text_attributes ADD COLUMN set_at timestamp with time zone NOT NULL DEFAULT '-infinity';
ALTER TABLE attributes.integer_attributes ADD COLUMN set_at timestamp with time zone NOT NULL DEFAULT '-infinity';
ALTER TABLE attributes.location_attributes ADD COLUMN set_at timestamp with time zone NOT NULL DEFAULT '-infinity';
ALTER TABLE attributes.birthdays ADD COLUMN set_at timestamp with time zone NOT NULL DEFAULT '-infinity';

-- Change default to NOW() so new inserts get the current timestamp automatically
ALTER TABLE attributes.text_attributes ALTER COLUMN set_at SET DEFAULT NOW();
ALTER TABLE attributes.integer_attributes ALTER COLUMN set_at SET DEFAULT NOW();
ALTER TABLE attributes.location_attributes ALTER COLUMN set_at SET DEFAULT NOW();
ALTER TABLE attributes.birthdays ALTER COLUMN set_at SET DEFAULT NOW();

COMMIT;
