-- Revert taylorbot-postgres:20240213_bump_version_1.20.0 from pg

BEGIN;

ALTER TABLE commands.commands
    ADD COLUMN unhandled_error_count bigint DEFAULT 0 NOT NULL;
ALTER TABLE commands.commands
    ADD COLUMN successful_use_count bigint DEFAULT 0 NOT NULL;

CREATE TABLE commands.user_groups (
    name text NOT NULL,
    access_level smallint NOT NULL,
    PRIMARY KEY (name)
);
CREATE TABLE guilds.guild_role_groups (
    guild_id text NOT NULL,
    role_id text NOT NULL,
    group_name text NOT NULL,
    PRIMARY KEY (guild_id, role_id),
    CONSTRAINT group_name_fk FOREIGN KEY (group_name) REFERENCES commands.user_groups(name),
    CONSTRAINT guild_id_fk FOREIGN KEY (guild_id) REFERENCES guilds.guilds(guild_id)
);

INSERT INTO commands.messages_of_the_day (message, priority_from, priority_to) VALUES
('Use `{prefix}roles` to see what roles you can get in a server!', NULL, NULL);

UPDATE configuration.application_info SET info_value = '1.19.0' WHERE info_key = 'product_version';

COMMIT;
