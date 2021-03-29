-- Revert taylorbot-postgres:0016_bump_version_1.10.0 from pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.9.0' WHERE info_key = 'product_version';

COMMIT;
