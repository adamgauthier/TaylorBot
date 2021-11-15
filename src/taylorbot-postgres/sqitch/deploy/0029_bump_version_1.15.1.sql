-- Deploy taylorbot-postgres:0029_bump_version_1.15.1 to pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.15.1' WHERE info_key = 'product_version';

DELETE FROM commands.messages_of_the_day WHERE message = 'TaylorBot 1.15.0 is out! Threads are now fully supported!';

UPDATE commands.messages_of_the_day SET
    message = 'If you have a **TaylorBot Plus** server, you can monitor deleted and edited messages. Use **/monitor deleted set** and **/monitor edited set** to get started!',
    priority_from = NULL,
    priority_to = NULL
WHERE message = 'TaylorBot 1.15.0 is out! If you have a **TaylorBot Plus** server, you can monitor deleted and edited messages. Use **/monitor deleted set** and **/monitor edited set** to get started!';

COMMIT;
