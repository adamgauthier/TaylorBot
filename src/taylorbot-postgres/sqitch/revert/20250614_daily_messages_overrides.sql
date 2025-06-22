-- Revert taylorbot-postgres:20250614_daily_messages_overrides from pg

BEGIN;

-- Drop new primary key
ALTER TABLE commands.messages_of_the_day DROP CONSTRAINT messages_of_the_day_pkey;

-- Add back original primary key
ALTER TABLE commands.messages_of_the_day ADD CONSTRAINT messages_of_the_day_pkey PRIMARY KEY (message);

-- Drop id column
ALTER TABLE commands.messages_of_the_day DROP COLUMN id;

COMMIT;
