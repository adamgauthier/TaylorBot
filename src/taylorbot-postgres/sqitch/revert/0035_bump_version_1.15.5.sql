-- Revert taylorbot-postgres:0035_bump_version_1.15.5 from pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.15.4' WHERE info_key = 'product_version';

COMMIT;
