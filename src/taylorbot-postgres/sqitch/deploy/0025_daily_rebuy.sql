-- Deploy taylorbot-postgres:0025_daily_rebuy to pg

BEGIN;

ALTER TABLE users.daily_payouts
    ADD COLUMN max_streak_count bigint DEFAULT 0 NOT NULL;

UPDATE users.daily_payouts SET max_streak_count = streak_count - 1;

COMMIT;
