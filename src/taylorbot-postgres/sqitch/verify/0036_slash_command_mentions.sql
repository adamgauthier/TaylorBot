-- Verify taylorbot-postgres:0036_slash_command_mentions on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT NOT EXISTS((SELECT FROM commands.messages_of_the_day WHERE
        position('**/' in message) > 0 OR position('`/' in message) > 0
    )));
END $$;

ROLLBACK;
