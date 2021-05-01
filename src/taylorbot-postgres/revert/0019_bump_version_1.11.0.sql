-- Revert taylorbot-postgres:0019_bump_version_1.11.0 from pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.10.1' WHERE info_key = 'product_version';

UPDATE commands.messages_of_the_day SET
    message = 'TaylorBot 1.10.1 is out! Try the **new Slash Command experience** by typing `/avatar`! No more typos in the command format or forgetting the server prefix!',
    priority_from = timestamp with time zone '2021-04-16',
    priority_to = timestamp with time zone '2021-04-19'
WHERE message = 'Try the **new Slash Command experience** by typing `/avatar`! No more typos in the command format or forgetting the server prefix!';

DELETE FROM commands.messages_of_the_day WHERE
    message = 'TaylorBot 1.11.0 is out! Use **/mod log set** to set up a channel to record moderator usage of **/kick** and `{prefix}jail`!'
;

COMMIT;
