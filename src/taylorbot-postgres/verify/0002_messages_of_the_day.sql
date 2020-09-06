-- Verify taylorbot-postgres:0002_messages_of_the_day on pg

BEGIN;

DO $$
DECLARE
    row_count bigint;
BEGIN
    row_count := (SELECT COUNT(*) FROM commands.messages_of_the_day WHERE
        message = 'You can gamble taypoints to win even more... or lose them! Use `{prefix}gamble` to get started.' OR
        message = 'Just like gambling, you can use `{prefix}heist` to heist your taypoints with friends and win even more!' OR
        message = 'Use `{prefix}setbirthday` to set your birthday and get points when the time comes!' OR
        message = 'Use `{prefix}time` to see what time it is for any user that has set their location!' OR
        message = 'TaylorBot has a variety of features, use `{prefix}help` to get started with interesting ones!' OR
        message = 'TaylorBot remembers the date when you first joined a server. Use `{prefix}joined` to see yours!' OR
        message = 'Use `{prefix}choose` to have TaylorBot make a decision for you!' OR
        message = 'Use `{prefix}status` to show off your Discord status, including your currently playing song on Spotify!' OR
        message = 'Start a poll and see what others think using `{prefix}poll`!' OR
        message = 'You can play rock paper scissors with TaylorBot using `{prefix}rps`, you''ll even get a reward if you win!' OR
        message = 'Are star signs really the one true science? Use `{prefix}horoscope` to get started.' OR
        message = 'Feeling generous? Use `{prefix}gift` to send some of your points to a friend!' OR
        message = 'Scared of what happens to your points if you lose your account? Use `{prefix}taypointwill add`!' OR
        message = 'TaylorBot is funded by the community, thanks to our Patreon supporters. Learn more with `{prefix}support`.' OR
        message = 'Set up your Last.fm account with `{prefix}fm set`, use `{prefix}fm` to show your now playing, use `{prefix}fm albums`, `{prefix}fm artists` and `{prefix}fm tracks` to show off what you listen to the most!'
    );
    ASSERT row_count = 15;
END $$;

ROLLBACK;
