SELECT guild_name, changed_at
FROM public.guild_names
WHERE guild_id = ${guild_id}
ORDER BY changed_at DESC
LIMIT ${max_rows};