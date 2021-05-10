-- Verify taylorbot-postgres:0005_update_1.8.0_message_of_the_day on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT EXISTS((SELECT 1 FROM commands.messages_of_the_day WHERE
        message = 'Use `{prefix}roles` to see what roles you can get in a server!' AND
        priority_from IS NULL AND priority_to IS NULL
    )));
END $$;

ROLLBACK;
