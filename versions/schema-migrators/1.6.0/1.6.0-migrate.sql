UPDATE commands.commands SET module_name = 'Random 🎲' WHERE module_name = 'random';
UPDATE commands.commands SET module_name = 'DiscordInfo 💬' WHERE module_name = 'discord' OR module_name = 'DiscordInfo';
UPDATE commands.commands SET module_name = 'Fun 🎭' WHERE module_name = 'fun';
UPDATE commands.commands SET module_name = 'Knowledge ❓' WHERE module_name = 'knowledge';
UPDATE commands.commands SET module_name = 'Media 📷' WHERE module_name = 'media';
UPDATE commands.commands SET module_name = 'Points 💰' WHERE module_name = 'points';
UPDATE commands.commands SET module_name = 'Reminders ⏰' WHERE module_name = 'reminders';
UPDATE commands.commands SET module_name = 'Stats 📊' WHERE module_name = 'stats';
UPDATE commands.commands SET module_name = 'Weather 🌦' WHERE module_name = 'weather';

CREATE TABLE guilds.jail_roles
(
    guild_id text NOT NULL,
    jail_role_id text NOT NULL,
    set_at timestamp without time zone NOT NULL,
    PRIMARY KEY (guild_id)
);

ALTER TABLE guilds.jail_roles OWNER to postgres;

GRANT ALL ON TABLE guilds.jail_roles TO taylorbot;

UPDATE commands.commands SET name = 'daily' WHERE name = 'dailypayout';
