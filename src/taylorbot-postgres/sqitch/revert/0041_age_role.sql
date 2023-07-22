-- Revert taylorbot-postgres:0041_age_role from pg

BEGIN;

DROP TABLE plus.age_roles;

COMMIT;
