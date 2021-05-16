-- Deploy taylorbot-postgres:0021_bump_version_1.11.1 to pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.11.1' WHERE info_key = 'product_version';

UPDATE commands.messages_of_the_day SET
    message = 'Use **/mod log set** to set up a channel to record moderation command usage!',
    priority_from = NULL,
    priority_to = NULL
WHERE message = 'TaylorBot 1.11.0 is out! Use **/mod log set** to set up a channel to record moderator usage of **/kick** and `{prefix}jail`!';

COMMIT;
