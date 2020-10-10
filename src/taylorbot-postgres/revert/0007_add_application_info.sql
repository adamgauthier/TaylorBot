-- Revert taylorbot-postgres:0007_add_application_info from pg

BEGIN;

DROP SCHEMA configuration CASCADE;

COMMIT;
