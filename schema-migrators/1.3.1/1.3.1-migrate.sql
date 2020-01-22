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
