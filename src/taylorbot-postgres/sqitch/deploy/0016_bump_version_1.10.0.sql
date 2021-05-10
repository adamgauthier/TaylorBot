-- Deploy taylorbot-postgres:0016_bump_version_1.10.0 to pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.10.0' WHERE info_key = 'product_version';

COMMIT;
