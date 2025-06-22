-- Deploy taylorbot-postgres:20250614_daily_messages_overrides to pg

BEGIN;

-- Delete all existing rows
DELETE FROM commands.messages_of_the_day;

-- Add new id column
ALTER TABLE commands.messages_of_the_day ADD COLUMN id uuid;

-- Drop existing primary key
ALTER TABLE commands.messages_of_the_day DROP CONSTRAINT messages_of_the_day_pkey;

-- Add new primary key on id
ALTER TABLE commands.messages_of_the_day ADD CONSTRAINT messages_of_the_day_pkey PRIMARY KEY (id);

COMMIT;
