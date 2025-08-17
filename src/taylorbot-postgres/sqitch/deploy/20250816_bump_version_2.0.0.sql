-- Deploy taylorbot-postgres:20250816_bump_version_2.0.0 to pg

BEGIN;

-- Rename gamble_stats table to risk_stats
ALTER TABLE users.gamble_stats RENAME TO risk_stats;

-- Rename columns in risk_stats table
ALTER TABLE users.risk_stats RENAME COLUMN gamble_win_count TO risk_win_count;
ALTER TABLE users.risk_stats RENAME COLUMN gamble_win_amount TO risk_win_amount;
ALTER TABLE users.risk_stats RENAME COLUMN gamble_lose_count TO risk_lose_count;
ALTER TABLE users.risk_stats RENAME COLUMN gamble_lose_amount TO risk_lose_amount;

-- Remove module_name column from commands.commands table
ALTER TABLE commands.commands DROP COLUMN module_name;

-- Update version
UPDATE configuration.application_info SET info_value = '2.0.0' WHERE info_key = 'product_version';

COMMIT;
