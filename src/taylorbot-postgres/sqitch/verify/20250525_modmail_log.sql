-- Verify taylorbot-postgres:20250525_modmail_log on pg

BEGIN;

DO $$
BEGIN
    -- Verify that all guilds with mod log channels but no modmail log channels have been migrated
    ASSERT NOT EXISTS (
        SELECT 1
        FROM moderation.mod_log_channels m
        LEFT JOIN moderation.mod_mail_log_channels mm ON m.guild_id = mm.guild_id
        WHERE mm.guild_id IS NULL
    ), 'Some guilds with mod log channels are missing modmail log channels';
END $$;

ROLLBACK;
