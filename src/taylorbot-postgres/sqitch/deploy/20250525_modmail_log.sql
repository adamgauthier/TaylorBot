-- Deploy taylorbot-postgres:20250525_modmail_log to pg

BEGIN;

-- Copy mod log channel configurations to modmail log channels for guilds that don't have a modmail log configured yet
INSERT INTO moderation.mod_mail_log_channels (guild_id, channel_id, added_at)
SELECT m.guild_id, m.channel_id, m.added_at
FROM moderation.mod_log_channels m
LEFT JOIN moderation.mod_mail_log_channels mm ON m.guild_id = mm.guild_id
WHERE mm.guild_id IS NULL;

COMMIT;
