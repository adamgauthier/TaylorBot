-- Deploy taylorbot-postgres:20240611_bump_version_1.21.0 to pg

BEGIN;

UPDATE configuration.application_info SET info_value = '1.21.0' WHERE info_key = 'product_version';

UPDATE commands.messages_of_the_day
SET message = 'Subscribe to [TaylorBot Plus](https://www.patreon.com/taylorbot) to get **exclusive features and taypoints**, learn more with </plus show:1246970937321066608> 💎'
WHERE message = 'TaylorBot is funded by the community, thanks to our TaylorBot Plus members. Learn more with `{prefix}plus`.';

UPDATE commands.messages_of_the_day
SET message = 'On **TaylorBot''s profile**, tap **Add App** -> **Add to Server** to add TaylorBot to your own server! 🤖'
WHERE message = 'Do you like TaylorBot? Do you want to add me to another server you''re in? Use the ''**Add to Server**'' button on my profile to get started!';

INSERT INTO commands.messages_of_the_day (message, priority_from, priority_to) VALUES (
'TaylorBot **1.21.0** is out! You can **use the bot everywhere on Discord** 🎉
- Add **TaylorBot** to your account, allowing you to use even in servers where **TaylorBot** is not added 🌐
- On **TaylorBot''s profile**, tap **Add App** -> **Use this app everywhere** ✅
- You can also go to **DMs** with your friends and type **/** to use **TaylorBot** commands ✨'
, timestamp with time zone '2024-06-12', timestamp with time zone '2024-06-14');


COMMIT;
