ALTER TABLE users.users
    ALTER COLUMN ignore_until DROP DEFAULT,
    ALTER COLUMN ignore_until TYPE timestamp with time zone USING TO_TIMESTAMP(ignore_until::double precision / 1000::double precision),
    ALTER COLUMN ignore_until SET DEFAULT NOW();

ALTER TABLE users.users
    ADD COLUMN is_bot boolean NOT NULL DEFAULT 'FALSE';

ALTER TABLE users.users
    ALTER COLUMN is_bot DROP DEFAULT;

ALTER TABLE guilds.text_channels
    ALTER COLUMN registered_at TYPE timestamp with time zone USING TO_TIMESTAMP(registered_at::double precision / 1000::double precision),
    ALTER COLUMN registered_at SET DEFAULT NOW();

ALTER TABLE guilds.guild_names
    ALTER COLUMN changed_at TYPE timestamp with time zone USING TO_TIMESTAMP(changed_at::double precision / 1000::double precision),
    ALTER COLUMN changed_at SET DEFAULT NOW();

ALTER TABLE users.usernames
    ALTER COLUMN changed_at TYPE timestamp with time zone USING TO_TIMESTAMP(changed_at::double precision / 1000::double precision),
    ALTER COLUMN changed_at SET DEFAULT NOW();