-- Revert taylorbot-postgres:0003_accessible_roles_group from pg

BEGIN;

ALTER TABLE guilds.guild_accessible_roles
    DROP COLUMN group_name;

ALTER TABLE guilds.guild_accessible_roles
    RENAME TO guild_special_roles;

COMMIT;
