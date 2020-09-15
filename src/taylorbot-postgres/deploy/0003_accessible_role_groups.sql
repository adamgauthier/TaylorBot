-- Deploy taylorbot-postgres:0003_accessible_roles_group to pg

BEGIN;

ALTER TABLE guilds.guild_special_roles
    RENAME TO guild_accessible_roles;

ALTER TABLE guilds.guild_accessible_roles
    ADD COLUMN group_name text DEFAULT NULL;

COMMIT;
