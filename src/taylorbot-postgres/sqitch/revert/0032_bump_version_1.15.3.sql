-- Revert taylorbot-postgres:0032_bump_version_1.15.3 from pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.15.2' WHERE info_key = 'product_version';

UPDATE commands.commands SET name = 'dailypayoutstreak' WHERE name = 'daily streak';
UPDATE commands.commands SET name = 'rankdailypayoutstreak' WHERE name = 'daily leaderboard';

COMMIT;
