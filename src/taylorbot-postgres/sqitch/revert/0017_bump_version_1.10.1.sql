-- Revert taylorbot-postgres:0017_bump_version_1.10.1 from pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.10.0' WHERE info_key = 'product_version';

DELETE FROM commands.messages_of_the_day WHERE
    message = 'TaylorBot 1.10.1 is out! Try the **new Slash Command experience** by typing `/avatar`! No more typos in the command format or forgetting the server prefix!'
;

COMMIT;
