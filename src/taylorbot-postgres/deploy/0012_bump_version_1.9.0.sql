-- Deploy taylorbot-postgres:0012_bump_version_1.9.0 to pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.9.0' WHERE info_key = 'product_version';

INSERT INTO commands.messages_of_the_day (message, priority_from, priority_to) VALUES
('TaylorBot 1.9.0 is out! You can now make your username history private with `{prefix}usernames private`!', timestamp with time zone '2021-01-18', timestamp with time zone '2020-01-20');

COMMIT;
