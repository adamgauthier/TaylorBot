-- Deploy taylorbot-postgres:0035_bump_version_1.15.5 to pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.15.5' WHERE info_key = 'product_version';

COMMIT;
