-- Revert taylorbot-postgres:0026_bump_version_1.14.0 from pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.13.0' WHERE info_key = 'product_version';

DELETE FROM commands.messages_of_the_day WHERE
    message = 'TaylorBot 1.14.0 is out! For a limited time, use **/daily claim** to get **more taypoints every day**!'
;

DELETE FROM commands.messages_of_the_day WHERE
    message = 'TaylorBot 1.14.0 is out! You can now re-buy a lost daily streak with **/daily rebuy**!'
;

UPDATE commands.messages_of_the_day SET
    message = 'TaylorBot 1.13.0 is out! Try the revamped reminder feature! Ask TaylorBot to remind you about something using **/remind add**.',
    priority_from = timestamp with time zone '2021-07-07',
    priority_to = timestamp with time zone '2021-07-09'
WHERE message = 'Ask TaylorBot to remind you about something using **/remind add**!';

UPDATE commands.messages_of_the_day SET message = 'Use `{prefix}choose` to have TaylorBot make a decision for you!'
WHERE message = 'Use **/choose** to have TaylorBot make a decision for you!';

COMMIT;
