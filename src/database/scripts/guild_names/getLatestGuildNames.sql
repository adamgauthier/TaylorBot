SELECT g.guild_name, g.guild_id
FROM
(SELECT guild_id, MAX(changed_at) AS max_changed_at
FROM public.guild_names
GROUP BY guild_id) AS maxed
JOIN public.guild_names AS g ON g.guild_id = maxed.guild_id AND g.changed_at = maxed.max_changed_at;