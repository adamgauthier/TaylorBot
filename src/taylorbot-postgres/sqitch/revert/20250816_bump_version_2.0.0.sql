-- Revert taylorbot-postgres:20250816_bump_version_2.0.0 from pg

BEGIN;

-- Restore module_name column to commands.commands table
ALTER TABLE commands.commands ADD COLUMN module_name text NOT NULL DEFAULT '';

-- Revert column names in gamble_stats table
ALTER TABLE users.risk_stats RENAME COLUMN risk_win_count TO gamble_win_count;
ALTER TABLE users.risk_stats RENAME COLUMN risk_win_amount TO gamble_win_amount;
ALTER TABLE users.risk_stats RENAME COLUMN risk_lose_count TO gamble_lose_count;
ALTER TABLE users.risk_stats RENAME COLUMN risk_lose_amount TO gamble_lose_amount;

-- Revert table name from risk_stats to gamble_stats
ALTER TABLE users.risk_stats RENAME TO gamble_stats;

-- Revert version
UPDATE configuration.application_info SET info_value = '1.21.0' WHERE info_key = 'product_version';

COMMIT;
