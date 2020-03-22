ALTER TABLE guilds.guild_members
    ALTER COLUMN first_joined_at DROP NOT NULL,
    ALTER COLUMN first_joined_at TYPE timestamp with time zone USING CASE WHEN first_joined_at = 9223372036854775807 THEN NULL ELSE TO_TIMESTAMP(first_joined_at::double precision / 1000::double precision) END;