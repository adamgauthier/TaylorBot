-- Verify taylorbot-postgres:base_schema on pg

BEGIN;

SELECT pg_catalog.has_schema_privilege('attributes', 'usage');
SELECT pg_catalog.has_schema_privilege('users', 'usage');
SELECT pg_catalog.has_schema_privilege('commands', 'usage');
SELECT pg_catalog.has_schema_privilege('guilds', 'usage');
SELECT pg_catalog.has_schema_privilege('checkers', 'usage');

ROLLBACK;
