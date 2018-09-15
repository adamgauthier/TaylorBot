ALTER TABLE guilds.text_channels
    RENAME is_logging TO is_log;

ALTER TABLE guilds.text_channels
    ADD COLUMN is_spam boolean NOT NULL DEFAULT FALSE;