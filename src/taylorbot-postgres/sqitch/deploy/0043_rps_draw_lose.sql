-- Deploy taylorbot-postgres:0043_rps_draw_lose to pg

BEGIN;

ALTER TABLE users.rps_stats
    ALTER COLUMN rps_win_count TYPE integer;

ALTER TABLE users.rps_stats
    ADD COLUMN rps_draw_count integer DEFAULT 0 NOT NULL;

UPDATE users.rps_stats SET rps_draw_count = rps_win_count;

ALTER TABLE users.rps_stats
    ALTER COLUMN rps_draw_count DROP DEFAULT;

ALTER TABLE users.rps_stats
    ADD COLUMN rps_lose_count integer DEFAULT 0 NOT NULL;

UPDATE users.rps_stats SET rps_lose_count = rps_win_count;

ALTER TABLE users.rps_stats
    ALTER COLUMN rps_lose_count DROP DEFAULT;

COMMIT;
