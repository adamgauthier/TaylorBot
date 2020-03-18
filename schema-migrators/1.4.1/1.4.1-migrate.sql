ALTER TABLE guilds.guilds
    ADD COLUMN guild_name text;

ALTER TABLE guilds.guilds
    ADD COLUMN previous_guild_name text;

UPDATE guilds.guilds SET guild_name = (
    SELECT g.guild_name
    FROM (
        SELECT guild_id, MAX(changed_at) AS max_changed_at
        FROM guilds.guild_names
        GROUP BY guild_id
    ) AS maxed
    JOIN guilds.guild_names AS g ON g.guild_id = maxed.guild_id AND g.changed_at = maxed.max_changed_at
    WHERE g.guild_id = guilds.guilds.guild_id
);

UPDATE guilds.guilds SET guild_name = '' WHERE guild_name IS NULL;

ALTER TABLE guilds.guilds
    ALTER COLUMN guild_name SET NOT NULL;
