-- Verify taylorbot-postgres:0016_bump_version_1.10.0 on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT info_value FROM configuration.application_info WHERE info_key = 'product_version') = '1.10.0';
END $$;

ROLLBACK;
