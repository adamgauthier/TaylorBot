ALTER TABLE users.users
    ADD COLUMN username text;

ALTER TABLE users.users
    ADD COLUMN previous_username text;

UPDATE users.users SET username = (
    SELECT u.username
    FROM (
        SELECT user_id, MAX(changed_at) AS max_changed_at
        FROM users.usernames
        GROUP BY user_id
    ) AS maxed
    JOIN users.usernames AS u ON u.user_id = maxed.user_id AND u.changed_at = maxed.max_changed_at
    WHERE u.user_id = users.users.user_id
);

UPDATE users.users SET username = '' WHERE username IS NULL;

ALTER TABLE users.users
    ALTER COLUMN username SET NOT NULL;

ALTER TABLE guilds.text_channels
    ADD COLUMN is_message_log boolean NOT NULL DEFAULT FALSE;

ALTER TABLE guilds.text_channels
    RENAME is_log TO is_member_log;

DELETE FROM guilds.channel_commands WHERE command_id = 'cleverbot';

DELETE FROM commands.commands WHERE name = 'cleverbot';
