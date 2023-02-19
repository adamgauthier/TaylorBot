-- Revert taylorbot-postgres:0037_slash_birthday from pg

BEGIN;

UPDATE commands.messages_of_the_day SET message = 'Are star signs really the one true science? Use `{prefix}horoscope` to get started.'
WHERE message = 'Are star signs really the one true science? Use </birthday horoscope:1016938623880400907> to get started.';

UPDATE commands.messages_of_the_day SET message = 'Use `{prefix}setbirthday` to set your birthday and get points when the time comes!'
WHERE message = 'Use </birthday set:1016938623880400907> to set your birthday and get points when the time comes!';

COMMIT;
