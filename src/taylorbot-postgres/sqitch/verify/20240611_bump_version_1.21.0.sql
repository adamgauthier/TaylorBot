-- Verify taylorbot-postgres:20240611_bump_version_1.21.0 on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT info_value FROM configuration.application_info WHERE info_key = 'product_version') = '1.21.0';

    ASSERT (SELECT NOT EXISTS(SELECT FROM commands.messages_of_the_day WHERE
        position('{prefix}plus' in message) > 0
    ));

    ASSERT (SELECT EXISTS(SELECT FROM commands.messages_of_the_day WHERE
        message = 'On **TaylorBot''s profile**, tap **Add App** -> **Add to Server** to add TaylorBot to your own server! 🤖'
    ));

    ASSERT (SELECT EXISTS(SELECT FROM commands.messages_of_the_day WHERE message =
'TaylorBot **1.21.0** is out! You can **use the bot everywhere on Discord** 🎉
- Add **TaylorBot** to your account, allowing you to use even in servers where **TaylorBot** is not added 🌐
- On **TaylorBot''s profile**, tap **Add App** -> **Use this app everywhere** ✅
- You can also go to **DMs** with your friends and type **/** to use **TaylorBot** commands ✨'
    ));
END $$;

ROLLBACK;
