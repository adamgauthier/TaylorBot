-- Verify taylorbot-postgres:0030_slash_lastfm on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT NOT EXISTS((SELECT FROM commands.commands WHERE
        name = 'lastfm'
    )));
END $$;

ROLLBACK;
