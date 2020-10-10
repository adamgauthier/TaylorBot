-- Verify taylorbot-postgres:0007_add_application_info on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT info_value FROM configuration.application_info WHERE info_key = 'product_version') = '1.8.0';
END $$;

ROLLBACK;
