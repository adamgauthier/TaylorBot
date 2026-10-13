-- Deploy taylorbot-postgres:20261012_user_ignore_state to pg

-- Deploy nullable ignore readers in commands-discord before this schema change.

BEGIN;

ALTER TABLE users.users
    ADD COLUMN created_at timestamp with time zone NOT NULL DEFAULT now(),
    ALTER COLUMN ignore_until DROP DEFAULT,
    ALTER COLUMN ignore_until DROP NOT NULL;

COMMENT ON COLUMN users.users.created_at IS
    'Row creation time; users predating this migration have the migration timestamp.';

COMMIT;
