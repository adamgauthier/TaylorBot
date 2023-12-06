-- Revert taylorbot-postgres:0043_bump_version_1.19.0 from pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.18.0' WHERE info_key = 'product_version';

UPDATE commands.messages_of_the_day
SET message = 'TaylorBot remembers the date when you first joined a server. Use `{prefix}joined` to see yours!'
WHERE message = 'TaylorBot remembers the date when you first joined a server. Use </server joined:1137547317549998130> to see yours!';

UPDATE commands.messages_of_the_day
SET message = 'TaylorBot keeps track of how active you are in each server. Use `{prefix}minutes` and `{prefix}rankminutes` to see time spent in a server!'
WHERE message = 'TaylorBot keeps track of how active you are in each server. Use </server minutes:1137547317549998130>, </server messages:1137547317549998130> and </server leaderboard:1137547317549998130> in you favorite server!';

COMMIT;
