-- Revert taylorbot-postgres:0008_bump_version_1.8.1 from pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.8.0' WHERE info_key = 'product_version';

DELETE FROM commands.messages_of_the_day WHERE
    message = 'Do you like TaylorBot? Do you want to add it to another server you''re in? Go to https://taylorbot.app/ to get started!'
;

COMMIT;
