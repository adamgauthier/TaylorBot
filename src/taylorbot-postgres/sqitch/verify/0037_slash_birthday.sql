-- Verify taylorbot-postgres:0037_slash_birthday on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT NOT EXISTS((SELECT FROM commands.messages_of_the_day WHERE
        position('{prefix}horoscope' in message) > 0 OR position('{prefix}setbirthday' in message) > 0
    )));
END $$;

ROLLBACK;
