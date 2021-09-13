-- Revert taylorbot-postgres:0029_bump_version_1.15.0 from pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.14.0' WHERE info_key = 'product_version';

DELETE FROM commands.messages_of_the_day WHERE
    message = 'TaylorBot 1.15.0 is out! Threads are now fully supported!'
;

DELETE FROM commands.messages_of_the_day WHERE
    message = 'TaylorBot 1.15.0 is out! If you have a **TaylorBot Plus** server, you can monitor deleted and edited messages. Use **/monitor deleted set** and **/monitor edited set** to get started!'
;

UPDATE commands.messages_of_the_day SET
    message = 'TaylorBot 1.14.0 is out! For a limited time, use **/daily claim** to get **more taypoints every day**!',
    priority_from = timestamp with time zone '2021-08-01',
    priority_to = timestamp with time zone '2021-08-04'
WHERE message = 'For a limited time, use **/daily claim** to get **more taypoints every day**!';

UPDATE commands.messages_of_the_day SET
    message = 'TaylorBot 1.14.0 is out! You can now re-buy a lost daily streak with **/daily rebuy**!',
    priority_from = timestamp with time zone '2021-08-05',
    priority_to = timestamp with time zone '2021-08-07'
WHERE message = 'Lost a longtime daily streak? Re-buy it with **/daily rebuy**!';

COMMIT;
