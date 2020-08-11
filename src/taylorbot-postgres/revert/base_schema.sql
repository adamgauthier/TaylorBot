-- Revert taylorbot-postgres:base_schema from pg

BEGIN;

DROP FUNCTION public._final_median CASCADE;
DROP FUNCTION public.zodiac;
DROP SCHEMA attributes CASCADE;
DROP SCHEMA users CASCADE;
DROP SCHEMA commands CASCADE;
DROP SCHEMA guilds CASCADE;
DROP SCHEMA checkers CASCADE;

COMMIT;
