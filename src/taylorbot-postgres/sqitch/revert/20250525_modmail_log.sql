-- Revert taylorbot-postgres:20250525_modmail_log from pg

BEGIN;

-- Remove modmail log channel configurations that were copied from mod log channels
DELETE FROM moderation.mod_mail_log_channels mm
WHERE EXISTS (
    SELECT 1 FROM moderation.mod_log_channels m
    WHERE m.guild_id = mm.guild_id
    AND m.channel_id = mm.channel_id
    AND m.added_at = mm.added_at
);

COMMIT;
