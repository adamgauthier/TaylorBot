SELECT 
    ranked.first_joined_at,
    ranked.rank
FROM
    (SELECT
        first_joined_at,
        user_id,
        rank() OVER (ORDER BY first_joined_at ASC) AS rank
    FROM public.guild_members
    WHERE 
        first_joined_at != 0 AND
        guild_id = ${guild_id}
    ) AS ranked
WHERE ranked.user_id = ${user_id};