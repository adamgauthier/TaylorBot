-- Deploy taylorbot-postgres:0017_bump_version_1.10.1 to pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.10.1' WHERE info_key = 'product_version';

INSERT INTO commands.messages_of_the_day (message, priority_from, priority_to) VALUES
('TaylorBot 1.10.1 is out! Try the **new Slash Command experience** by typing `/avatar`! No more typos in the command format or forgetting the server prefix!', timestamp with time zone '2021-04-16', timestamp with time zone '2021-04-19');

COMMIT;
