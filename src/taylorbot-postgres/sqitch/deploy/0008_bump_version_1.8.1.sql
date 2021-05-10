-- Deploy taylorbot-postgres:0008_bump_version_1.8.1 to pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.8.1' WHERE info_key = 'product_version';

INSERT INTO commands.messages_of_the_day (message) VALUES
('Do you like TaylorBot? Do you want to add it to another server you''re in? Go to https://taylorbot.app/ to get started!');

COMMIT;
