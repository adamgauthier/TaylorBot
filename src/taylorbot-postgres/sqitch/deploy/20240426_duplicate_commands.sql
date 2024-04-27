-- Deploy taylorbot-postgres:20240426_duplicate_commands to pg

BEGIN;

UPDATE guilds.channel_commands SET command_id = 'rps play' WHERE command_id = 'rps';
UPDATE guilds.guild_commands SET command_name = 'rps play' WHERE command_name = 'rps';
DELETE FROM commands.commands WHERE name = 'rps';

UPDATE guilds.channel_commands SET command_id = 'rps play' WHERE command_id = 'rockpaperscissors';
UPDATE guilds.guild_commands SET command_name = 'rps play' WHERE command_name = 'rockpaperscissors';
DELETE FROM commands.commands WHERE name = 'rockpaperscissors';

DELETE FROM guilds.channel_commands WHERE command_id = 'clearfavoritesongs';
DELETE FROM guilds.guild_commands WHERE command_name = 'clearfavoritesongs';
DELETE FROM commands.commands WHERE name = 'clearfavoritesongs';

DELETE FROM guilds.channel_commands WHERE command_id = 'setsnapchat';
DELETE FROM guilds.guild_commands WHERE command_name = 'setsnapchat';
DELETE FROM commands.commands WHERE name = 'setsnapchat';

DELETE FROM guilds.channel_commands WHERE command_id = 'clearsnapchat';
DELETE FROM guilds.guild_commands WHERE command_name = 'clearsnapchat';
DELETE FROM commands.commands WHERE name = 'clearsnapchat';

DELETE FROM guilds.channel_commands WHERE command_id = 'listsnapchat';
DELETE FROM guilds.guild_commands WHERE command_name = 'listsnapchat';
DELETE FROM commands.commands WHERE name = 'listsnapchat';

DELETE FROM guilds.channel_commands WHERE command_id = 'clearinstagram';
DELETE FROM guilds.guild_commands WHERE command_name = 'clearinstagram';
DELETE FROM commands.commands WHERE name = 'clearinstagram';

DELETE FROM guilds.channel_commands WHERE command_id = 'setinstagram';
DELETE FROM guilds.guild_commands WHERE command_name = 'setinstagram';
DELETE FROM commands.commands WHERE name = 'setinstagram';

DELETE FROM guilds.channel_commands WHERE command_id = 'setfav';
DELETE FROM guilds.guild_commands WHERE command_name = 'setfav';
DELETE FROM commands.commands WHERE name = 'setfav';

DELETE FROM guilds.channel_commands WHERE command_id = 'fav';
DELETE FROM guilds.guild_commands WHERE command_name = 'fav';
DELETE FROM commands.commands WHERE name = 'fav';

DELETE FROM guilds.channel_commands WHERE command_id = 'perfectrolls';
DELETE FROM guilds.guild_commands WHERE command_name = 'perfectrolls';
DELETE FROM commands.commands WHERE name = 'perfectrolls';

DELETE FROM guilds.channel_commands WHERE command_id = 'rankdailypayoutstreak';
DELETE FROM guilds.guild_commands WHERE command_name = 'rankdailypayoutstreak';
DELETE FROM commands.commands WHERE name = 'rankdailypayoutstreak';

DELETE FROM guilds.channel_commands WHERE command_id = 'gamblefails';
DELETE FROM guilds.guild_commands WHERE command_name = 'gamblefails';
DELETE FROM commands.commands WHERE name = 'gamblefails';

DELETE FROM guilds.channel_commands WHERE command_id = 'gamblelosses';
DELETE FROM guilds.guild_commands WHERE command_name = 'gamblelosses';
DELETE FROM commands.commands WHERE name = 'gamblelosses';

DELETE FROM guilds.channel_commands WHERE command_id = 'gambleprofits';
DELETE FROM guilds.guild_commands WHERE command_name = 'gambleprofits';
DELETE FROM commands.commands WHERE name = 'gambleprofits';

DELETE FROM guilds.channel_commands WHERE command_id = 'heistfails';
DELETE FROM guilds.guild_commands WHERE command_name = 'heistfails';
DELETE FROM commands.commands WHERE name = 'heistfails';

DELETE FROM guilds.channel_commands WHERE command_id = 'heistlosses';
DELETE FROM guilds.guild_commands WHERE command_name = 'heistlosses';
DELETE FROM commands.commands WHERE name = 'heistlosses';

DELETE FROM guilds.channel_commands WHERE command_id = 'heistprofits';
DELETE FROM guilds.guild_commands WHERE command_name = 'heistprofits';
DELETE FROM commands.commands WHERE name = 'heistprofits';

DELETE FROM guilds.channel_commands WHERE command_id = 'dailypayoutstreak';
DELETE FROM guilds.guild_commands WHERE command_name = 'dailypayoutstreak';
DELETE FROM commands.commands WHERE name = 'dailypayoutstreak';

UPDATE guilds.channel_commands SET command_id = 'wolframalpha' WHERE command_id = 'wolfram';
UPDATE guilds.guild_commands SET command_name = 'wolframalpha' WHERE command_name = 'wolfram';
DELETE FROM commands.commands WHERE name = 'wolfram';

UPDATE commands.messages_of_the_day
SET message = 'Don''t recognize someone? Use </usernames show:1214478089883619378> to check if they changed their username! Use </usernames visibility:1214478089883619378> to make your history private! 🔍'
WHERE message = 'Don''t recognize someone? They might have changed their name, use `{prefix}usernames` to see their username history. Use `{prefix}usernames private` to make yours private!';

COMMIT;
