-- Revert taylorbot-postgres:0021_bump_version_1.11.1 from pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.11.0' WHERE info_key = 'product_version';

UPDATE commands.messages_of_the_day SET
    message = 'TaylorBot 1.11.0 is out! Use **/mod log set** to set up a channel to record moderator usage of **/kick** and `{prefix}jail`!',
    priority_from = timestamp with time zone '2021-05-02',
    priority_to = timestamp with time zone '2021-05-04'
WHERE message = 'Use **/mod log set** to set up a channel to record moderation command usage!';

COMMIT;
