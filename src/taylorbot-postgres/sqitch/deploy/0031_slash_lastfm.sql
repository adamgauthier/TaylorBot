-- Deploy taylorbot-postgres:0030_slash_lastfm to pg

BEGIN;

UPDATE commands.commands SET name = 'lastfm current' WHERE name = 'lastfm';

COMMIT;
