-- Deploy taylorbot-postgres:0019_bump_version_1.11.0 to pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.11.0' WHERE info_key = 'product_version';

UPDATE commands.messages_of_the_day SET
    message = 'Try the **new Slash Command experience** by typing `/avatar`! No more typos in the command format or forgetting the server prefix!',
    priority_from = NULL,
    priority_to = NULL
WHERE message = 'TaylorBot 1.10.1 is out! Try the **new Slash Command experience** by typing `/avatar`! No more typos in the command format or forgetting the server prefix!';

INSERT INTO commands.messages_of_the_day (message, priority_from, priority_to) VALUES
('TaylorBot 1.11.0 is out! Use **/mod log set** to set up a channel to record moderator usage of **/kick** and `{prefix}jail`!', timestamp with time zone '2021-05-02', timestamp with time zone '2021-05-04');

COMMIT;
