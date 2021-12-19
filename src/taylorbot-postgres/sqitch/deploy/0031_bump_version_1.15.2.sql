-- Deploy taylorbot-postgres:0031_bump_version_1.15.2 to pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.15.2' WHERE info_key = 'product_version';

UPDATE commands.messages_of_the_day
SET message = 'Set up your Last.fm account with **/lastfm set**, use **/lastfm current** to show your now playing, use **/lastfm albums**, **/lastfm artists** and **/lastfm tracks** to show off what you listen to!'
WHERE message = 'Set up your Last.fm account with `{prefix}fm set`, use `{prefix}fm` to show your now playing, use `{prefix}fm albums`, `{prefix}fm artists` and `{prefix}fm tracks` to show off what you listen to the most!';

COMMIT;
