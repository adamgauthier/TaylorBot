-- Revert taylorbot-postgres:0041_bump_version_1.18.0 from pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.17.0' WHERE info_key = 'product_version';

DELETE FROM commands.messages_of_the_day WHERE
    message = 'Use </location weather:1141925890448691270> to see the current weather where you or your friend are!'
;

UPDATE commands.messages_of_the_day
SET message = 'Feeling generous? Use `{prefix}gift` to send some of your points to a friend!'
WHERE message = 'Feeling generous? Use </taypoints gift:1103846727880028180> to send some of your taypoints to a friend!';

UPDATE commands.messages_of_the_day
SET message = 'Use `{prefix}time` to see what time it is for any user that has set their location!'
WHERE message = 'Use </location time:1141925890448691270> to see what time it is for your friend in a different part of the world!';

COMMIT;
