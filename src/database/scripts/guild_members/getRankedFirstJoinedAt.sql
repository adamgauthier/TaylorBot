SELECT 
    ranked.first_joined_at,
    ranked.rank
FROM
    (SELECT
        first_joined_at,
        user_id,
        rank() OVER (ORDER BY first_joined_at ASC) AS rank
    FROM guilds.guild_members
    WHERE 
        guild_id = ${guild_id}
    ) AS ranked
WHERE ranked.user_id = ${user_id};