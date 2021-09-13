-- Deploy taylorbot-postgres:0029_bump_version_1.15.0 to pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.15.0' WHERE info_key = 'product_version';

INSERT INTO commands.messages_of_the_day (message, priority_from, priority_to) VALUES
('TaylorBot 1.15.0 is out! Threads are now fully supported!', timestamp with time zone '2021-09-13', timestamp with time zone '2021-09-15');

INSERT INTO commands.messages_of_the_day (message, priority_from, priority_to) VALUES
('TaylorBot 1.15.0 is out! If you have a **TaylorBot Plus** server, you can monitor deleted and edited messages. Use **/monitor deleted set** and **/monitor edited set** to get started!', timestamp with time zone '2021-09-16', timestamp with time zone '2021-09-17');

UPDATE commands.messages_of_the_day SET
    message = 'For a limited time, use **/daily claim** to get **more taypoints every day**!',
    priority_from = NULL,
    priority_to = NULL
WHERE message = 'TaylorBot 1.14.0 is out! For a limited time, use **/daily claim** to get **more taypoints every day**!';

UPDATE commands.messages_of_the_day SET
    message = 'Lost a longtime daily streak? Re-buy it with **/daily rebuy**!',
    priority_from = NULL,
    priority_to = NULL
WHERE message = 'TaylorBot 1.14.0 is out! You can now re-buy a lost daily streak with **/daily rebuy**!';

COMMIT;
