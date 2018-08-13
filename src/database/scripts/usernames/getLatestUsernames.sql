SELECT u.username, u.user_id
FROM
(SELECT user_id, MAX(changed_at) AS max_changed_at
FROM users.usernames
GROUP BY user_id) AS maxed
JOIN users.usernames AS u ON u.user_id = maxed.user_id AND u.changed_at = maxed.max_changed_at;