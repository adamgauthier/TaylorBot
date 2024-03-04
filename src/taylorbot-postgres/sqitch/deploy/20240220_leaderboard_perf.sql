-- Deploy taylorbot-postgres:20240220_leaderboard_perf to pg

BEGIN;

CREATE INDEX IF NOT EXISTS idx_members_last_spoke ON guilds.guild_members (last_spoke_at DESC);

ALTER TABLE guilds.guild_members
    DROP COLUMN minutes_milestone;

CREATE INDEX IF NOT EXISTS idx_members_messages ON guilds.guild_members (guild_id, message_count DESC)
INCLUDE (user_id)
WHERE alive = TRUE AND message_count > 0;

CREATE INDEX IF NOT EXISTS idx_members_minutes ON guilds.guild_members (guild_id, minute_count DESC)
INCLUDE (user_id)
WHERE alive = TRUE AND minute_count > 0;

ALTER TABLE guilds.guild_members
    ADD COLUMN last_known_taypoint_count bigint DEFAULT NULL;

CREATE INDEX IF NOT EXISTS idx_members_taypoints ON guilds.guild_members (guild_id, last_known_taypoint_count DESC)
INCLUDE (user_id)
WHERE alive = TRUE AND last_known_taypoint_count IS NOT NULL;


CREATE MATERIALIZED VIEW attributes.birthday_calendar_6months AS
SELECT u.user_id, username, next_birthday FROM
(
    SELECT user_id, next_birthday FROM
    (
        SELECT
            user_id,
            CASE
                WHEN normalized_birthday < CURRENT_DATE
                THEN (normalized_birthday + INTERVAL '1 YEAR')
                ELSE normalized_birthday
            END AS next_birthday
        FROM
        (
            SELECT
                user_id,
                make_date(
                    date_part('year', CURRENT_DATE)::int,
                    date_part('month', birthday)::int,
                    CASE
                        WHEN date_part('month', birthday)::int = 2 AND date_part('day', birthday)::int = 29
                        THEN 28
                        ELSE date_part('day', birthday)::int
                    END
                ) AS normalized_birthday
            FROM attributes.birthdays
            WHERE is_private = FALSE
        ) public_normalized
    ) public_upcoming
    WHERE public_upcoming.next_birthday BETWEEN CURRENT_DATE AND (CURRENT_DATE + INTERVAL '6 MONTHS')
) calendar
JOIN users.users u ON u.user_id = calendar.user_id
WITH DATA;

CREATE UNIQUE INDEX idx_calendar_user_ids ON attributes.birthday_calendar_6months (user_id);

COMMIT;
