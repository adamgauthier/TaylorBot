-- Revert taylorbot-postgres:0025_daily_rebuy from pg

BEGIN;

ALTER TABLE users.daily_payouts
    DROP COLUMN max_streak_count;

COMMIT;
