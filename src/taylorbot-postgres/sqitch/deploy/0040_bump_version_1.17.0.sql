-- Deploy taylorbot-postgres:0040_bump_version_1.17.0 to pg

BEGIN;

UPDATE commands.messages_of_the_day
SET message = 'Start a poll and see what others think using </poll:1125515185138958467>!'
WHERE message = 'Start a poll and see what others think using `{prefix}poll`!';

UPDATE configuration.application_info SET info_value = '1.17.0' WHERE info_key = 'product_version';

COMMIT;
