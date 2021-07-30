-- Deploy taylorbot-postgres:0026_bump_version_1.14.0 to pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.14.0' WHERE info_key = 'product_version';

INSERT INTO commands.messages_of_the_day (message, priority_from, priority_to) VALUES
('TaylorBot 1.14.0 is out! For a limited time, use **/daily claim** to get **more taypoints every day**!', timestamp with time zone '2021-08-01', timestamp with time zone '2021-08-04');

INSERT INTO commands.messages_of_the_day (message, priority_from, priority_to) VALUES
('TaylorBot 1.14.0 is out! You can now re-buy a lost daily streak with **/daily rebuy**!', timestamp with time zone '2021-08-05', timestamp with time zone '2021-08-07');

UPDATE commands.messages_of_the_day SET
    message = 'Ask TaylorBot to remind you about something using **/remind add**!',
    priority_from = NULL,
    priority_to = NULL
WHERE message = 'TaylorBot 1.13.0 is out! Try the revamped reminder feature! Ask TaylorBot to remind you about something using **/remind add**.';

UPDATE commands.messages_of_the_day SET message = 'Use **/choose** to have TaylorBot make a decision for you!'
WHERE message = 'Use `{prefix}choose` to have TaylorBot make a decision for you!';

COMMIT;
