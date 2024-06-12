-- Revert taylorbot-postgres:20240611_bump_version_1.21.0 from pg

BEGIN;

DELETE FROM commands.messages_of_the_day
WHERE message =
'TaylorBot **1.21.0** is out! You can **use the bot everywhere on Discord** 🎉
- Add **TaylorBot** to your account, allowing you to use even in servers where **TaylorBot** is not added 🌐
- On **TaylorBot''s profile**, tap **Add App** -> **Use this app everywhere** ✅
- You can also go to **DMs** with your friends and type **/** to use **TaylorBot** commands ✨';

UPDATE commands.messages_of_the_day
SET message = 'Do you like TaylorBot? Do you want to add me to another server you''re in? Use the ''**Add to Server**'' button on my profile to get started!'
WHERE message = 'On **TaylorBot''s profile**, tap **Add App** -> **Add to Server** to add TaylorBot to your own server! 🤖';

UPDATE commands.messages_of_the_day
SET message = 'TaylorBot is funded by the community, thanks to our TaylorBot Plus members. Learn more with `{prefix}plus`.'
WHERE message = 'Subscribe to [TaylorBot Plus](https://www.patreon.com/taylorbot) to get **exclusive features and taypoints**, learn more with </plus show:1246970937321066608> 💎';

UPDATE configuration.application_info SET info_value = '1.20.0' WHERE info_key = 'product_version';

COMMIT;
