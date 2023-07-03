-- Revert taylorbot-postgres:0040_bump_version_1.17.0 from pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.16.0' WHERE info_key = 'product_version';

UPDATE commands.messages_of_the_day
SET message = 'Start a poll and see what others think using `{prefix}poll`!'
WHERE message = 'Start a poll and see what others think using </poll:1125515185138958467>!';

COMMIT;
