-- Deploy taylorbot-postgres:0032_bump_version_1.15.3 to pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.15.3' WHERE info_key = 'product_version';

UPDATE commands.commands SET name = 'daily streak' WHERE name = 'dailypayoutstreak';
UPDATE commands.commands SET name = 'daily leaderboard' WHERE name = 'rankdailypayoutstreak';

COMMIT;
