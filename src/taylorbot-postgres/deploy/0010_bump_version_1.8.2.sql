-- Deploy taylorbot-postgres:0010_bump_version_1.8.2 to pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.8.2' WHERE info_key = 'product_version';

DELETE FROM commands.messages_of_the_day WHERE
    message = 'Use `{prefix}status` to show off your Discord status, including your currently playing song on Spotify!'
;

COMMIT;
