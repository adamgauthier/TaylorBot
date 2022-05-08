-- Verify taylorbot-postgres:0035_bump_version_1.15.5 on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT info_value FROM configuration.application_info WHERE info_key = 'product_version') = '1.15.5';
END $$;

ROLLBACK;
