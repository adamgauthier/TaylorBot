-- Deploy taylorbot-postgres:20240213_bump_version_1.20.0 to pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.20.0' WHERE info_key = 'product_version';

DELETE FROM commands.messages_of_the_day WHERE message = 'Use `{prefix}roles` to see what roles you can get in a server!';

DROP TABLE guilds.guild_role_groups;
DROP TABLE commands.user_groups;

ALTER TABLE commands.commands DROP COLUMN successful_use_count;
ALTER TABLE commands.commands DROP COLUMN unhandled_error_count;

COMMIT;
