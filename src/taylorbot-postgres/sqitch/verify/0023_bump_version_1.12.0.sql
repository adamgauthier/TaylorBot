-- Verify taylorbot-postgres:0023_bump_version_1.12.0 on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT info_value FROM configuration.application_info WHERE info_key = 'product_version') = '1.12.0';

    ASSERT (SELECT EXISTS((SELECT FROM commands.messages_of_the_day WHERE
        message = 'TaylorBot 1.12.0 is out! You can now send and receive messages through your moderation team. As a moderator, use **/mod mail message-user** and as a user, use **/mod mail message-mods**!'
    )));
END $$;

ROLLBACK;
