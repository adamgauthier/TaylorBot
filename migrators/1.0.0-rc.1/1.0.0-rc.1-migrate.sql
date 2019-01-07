ALTER TABLE guilds.guild_members
    RENAME taypoint_count TO experience;

ALTER TABLE users.users
    ADD COLUMN taypoint_count bigint NOT NULL DEFAULT 0;