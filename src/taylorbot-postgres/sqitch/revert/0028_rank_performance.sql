-- Revert taylorbot-postgres:0028_rank_performance from pg

BEGIN;

DROP INDEX guilds.idx_members_first_joined_at;

COMMIT;
