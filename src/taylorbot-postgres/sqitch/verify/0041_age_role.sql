-- Verify taylorbot-postgres:0041_age_role on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT EXISTS(
        SELECT FROM information_schema.tables
        WHERE table_schema = 'plus' AND table_name = 'age_roles'
    ));

    ASSERT (SELECT NOT EXISTS(
        SELECT FROM attributes.integer_attributes
        WHERE attribute_id = 'age'
    ));
END $$;

ROLLBACK;
