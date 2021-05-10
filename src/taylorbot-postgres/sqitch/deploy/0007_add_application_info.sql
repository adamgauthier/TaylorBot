-- Deploy taylorbot-postgres:0007_add_application_info to pg

BEGIN;

CREATE SCHEMA configuration;

CREATE TABLE configuration.application_info (
    info_key text NOT NULL,
    info_value text NOT NULL,
    PRIMARY KEY (info_key)
);

INSERT INTO configuration.application_info (info_key, info_value) VALUES ('product_version', '1.8.0');

COMMIT;
