-- Revert taylorbot-postgres:0012_bump_version_1.9.0 from pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.8.2' WHERE info_key = 'product_version';

DELETE FROM commands.messages_of_the_day WHERE
    message = 'TaylorBot 1.9.0 is out! You can now make your username history private with `{prefix}usernames private`!'
;

COMMIT;
