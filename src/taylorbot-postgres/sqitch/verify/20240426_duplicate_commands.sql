-- Verify taylorbot-postgres:20240426_duplicate_commands on pg

BEGIN;

DO $$
BEGIN
    ASSERT (SELECT NOT EXISTS(SELECT FROM commands.messages_of_the_day WHERE
        position('{prefix}usernames' in message) > 0
    ));

    ASSERT (SELECT NOT EXISTS(
        SELECT FROM commands.commands AS c1
        WHERE EXISTS (
            SELECT 1
            FROM commands.commands AS c2
            WHERE (c1.name = ANY(c2.aliases) OR c2.name = ANY(c1.aliases)) AND c2.name <> c1.name
        )
    ));
END $$;


ROLLBACK;
