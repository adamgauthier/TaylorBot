ALTER TABLE users.users
    ALTER COLUMN ignore_until DROP DEFAULT,
    ALTER COLUMN ignore_until TYPE timestamp with time zone USING TO_TIMESTAMP(ignore_until::double precision / 1000::double precision),
    ALTER COLUMN ignore_until SET DEFAULT NOW();