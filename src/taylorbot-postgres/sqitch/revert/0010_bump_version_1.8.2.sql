-- Revert taylorbot-postgres:0010_bump_version_1.8.2 from pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.8.1' WHERE info_key = 'product_version';

INSERT INTO commands.messages_of_the_day (message) VALUES
('Use `{prefix}status` to show off your Discord status, including your currently playing song on Spotify!');

COMMIT;
