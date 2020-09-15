-- Verify taylorbot-postgres:0003_accessible_roles_group on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT EXISTS(
        SELECT FROM pg_tables
        WHERE schemaname = 'guilds' AND tablename  = 'guild_accessible_roles'
    ));
    ASSERT (SELECT EXISTS(
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = 'guilds' AND table_name = 'guild_accessible_roles' AND column_name = 'group_name'
    ));
END $$;

ROLLBACK;
