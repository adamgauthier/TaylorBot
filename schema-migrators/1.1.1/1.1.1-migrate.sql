ALTER TABLE guilds.guild_members
    ALTER COLUMN last_spoke_at DROP NOT NULL,
    ALTER COLUMN last_spoke_at DROP DEFAULT,
    ALTER COLUMN last_spoke_at TYPE timestamp with time zone USING CASE WHEN last_spoke_at = 0 THEN NULL ELSE TO_TIMESTAMP(last_spoke_at::double precision / 1000::double precision) END,
    ALTER COLUMN last_spoke_at SET DEFAULT NULL;