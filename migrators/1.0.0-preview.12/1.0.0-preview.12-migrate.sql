ALTER TABLE guilds.text_channels
    RENAME is_logging TO is_log;

ALTER TABLE guilds.text_channels
    ADD COLUMN is_spam boolean NOT NULL DEFAULT FALSE;

ALTER TABLE guilds.guild_members
    ADD COLUMN message_count integer NOT NULL DEFAULT 0;

ALTER TABLE guilds.guild_members
    ADD COLUMN word_count integer NOT NULL DEFAULT 0;

ALTER TABLE guilds.text_channels
    RENAME messages_count TO message_count;