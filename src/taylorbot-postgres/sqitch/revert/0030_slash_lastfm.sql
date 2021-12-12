-- Revert taylorbot-postgres:0030_slash_lastfm from pg

BEGIN;

UPDATE commands.commands SET name = 'lastfm' WHERE name = 'lastfm current';

COMMIT;
