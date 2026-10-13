-- Revert taylorbot-postgres:20261012_user_ignore_state from pg

BEGIN;

UPDATE users.users
SET ignore_until = TIMESTAMP WITH TIME ZONE '1970-01-01 00:00:00+00'
WHERE ignore_until IS NULL;

ALTER TABLE users.users
    ALTER COLUMN ignore_until SET DEFAULT now(),
    ALTER COLUMN ignore_until SET NOT NULL,
    DROP COLUMN created_at;

COMMIT;
