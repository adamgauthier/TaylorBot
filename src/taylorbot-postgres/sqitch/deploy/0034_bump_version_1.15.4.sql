-- Deploy taylorbot-postgres:0033_bump_version_1.15.4 to pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.15.4' WHERE info_key = 'product_version';

UPDATE commands.messages_of_the_day
SET message = 'Do you like TaylorBot? Do you want to add me to another server you''re in? Use the ''**Add to Server**'' button on my profile to get started!'
WHERE message = 'Do you like TaylorBot? Do you want to add it to another server you''re in? Go to https://taylorbot.app/ to get started!';

COMMIT;
