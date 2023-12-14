-- Revert taylorbot-postgres:0043_rps_draw_lose from pg

BEGIN;

ALTER TABLE users.rps_stats
    DROP COLUMN rps_lose_count;

ALTER TABLE users.rps_stats
    DROP COLUMN rps_draw_count;

ALTER TABLE users.rps_stats
    ALTER COLUMN rps_win_count TYPE bigint;

COMMIT;
