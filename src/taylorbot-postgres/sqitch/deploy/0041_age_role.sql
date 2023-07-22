-- Deploy taylorbot-postgres:0041_age_role to pg

BEGIN;

CREATE TABLE plus.age_roles (
    guild_id text NOT NULL,
    age_role_id text NOT NULL,
    minimum_age int NOT NULL,
    set_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (guild_id, age_role_id)
);

DELETE FROM attributes.integer_attributes WHERE attribute_id = 'age';

COMMIT;
