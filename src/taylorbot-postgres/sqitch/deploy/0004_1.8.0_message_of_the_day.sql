-- Deploy taylorbot-postgres:0004_1.8.0_message_of_the_day to pg

BEGIN;

INSERT INTO commands.messages_of_the_day (message, priority_from, priority_to) VALUES
('TaylorBot 1.8.0 is out! Try the new `{prefix}roles` command to see what roles you can get in a server!', timestamp with time zone '2020-09-18', timestamp with time zone '2020-09-20');

COMMIT;
