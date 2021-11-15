-- Revert taylorbot-postgres:0029_bump_version_1.15.1 from pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.15.0' WHERE info_key = 'product_version';

INSERT INTO commands.messages_of_the_day (message, priority_from, priority_to) VALUES
('TaylorBot 1.15.0 is out! Threads are now fully supported!', timestamp with time zone '2021-09-13', timestamp with time zone '2021-09-15');

UPDATE commands.messages_of_the_day SET
    message = 'TaylorBot 1.15.0 is out! If you have a **TaylorBot Plus** server, you can monitor deleted and edited messages. Use **/monitor deleted set** and **/monitor edited set** to get started!',
    priority_from = timestamp with time zone '2021-09-16',
    priority_to = timestamp with time zone '2021-09-17'
WHERE message = 'If you have a **TaylorBot Plus** server, you can monitor deleted and edited messages. Use **/monitor deleted set** and **/monitor edited set** to get started!';

COMMIT;
