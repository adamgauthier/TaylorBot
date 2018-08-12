INSERT INTO guilds.guild_commands (guild_id, command_name, disabled) 
VALUES (${guild_id}, ${command_name}, ${disabled})
ON CONFLICT (guild_id, command_name) DO UPDATE 
  SET disabled = excluded.disabled;