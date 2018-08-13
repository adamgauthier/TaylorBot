SELECT username, changed_at
FROM users.usernames
WHERE user_id = ${user_id}
ORDER BY changed_at DESC
LIMIT ${max_rows};