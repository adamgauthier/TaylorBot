-- Verify taylorbot-postgres:0031_bump_version_1.15.2 on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT info_value FROM configuration.application_info WHERE info_key = 'product_version') = '1.15.2';

    ASSERT (SELECT EXISTS((SELECT FROM commands.messages_of_the_day WHERE
        message = 'Set up your Last.fm account with **/lastfm set**, use **/lastfm current** to show your now playing, use **/lastfm albums**, **/lastfm artists** and **/lastfm tracks** to show off what you listen to!'
    )));
END $$;

ROLLBACK;
