CREATE TABLE users.heist_stats
(
    user_id text NOT NULL,
    heist_win_count bigint NOT NULL DEFAULT 0,
    heist_win_amount bigint NOT NULL DEFAULT 0,
    heist_lose_count bigint NOT NULL DEFAULT 0,
    heist_lose_amount bigint NOT NULL DEFAULT 0,
    PRIMARY KEY (user_id),
    CONSTRAINT user_id_fk FOREIGN KEY (user_id)
        REFERENCES users.users (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
);

ALTER TABLE users.heist_stats OWNER to postgres;

GRANT ALL ON TABLE users.heist_stats TO taylorbot;

CREATE TABLE attributes.birthdays
(
    user_id text NOT NULL,
    birthday date NOT NULL,
    last_reward_at timestamp with time zone DEFAULT NULL,
    is_private boolean,
    PRIMARY KEY (user_id),
    CONSTRAINT user_id_fk FOREIGN KEY (user_id)
        REFERENCES users.users (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
);

ALTER TABLE attributes.birthdays OWNER to postgres;

GRANT ALL ON TABLE attributes.birthdays TO taylorbot;

ALTER TABLE checkers.reddit_checker
    ALTER COLUMN last_created TYPE timestamp with time zone USING TO_TIMESTAMP(last_created);

CREATE OR REPLACE FUNCTION zodiac(birthday date) RETURNS text AS $$
    SELECT
    CASE  
    WHEN (birthday_month = 3 AND birthday_day >= 21) OR (birthday_month = 4 AND birthday_day <= 19) THEN 'Aries'
    WHEN (birthday_month = 4 AND birthday_day >= 20) OR (birthday_month = 5 AND birthday_day <= 20) THEN 'Taurus'
    WHEN (birthday_month = 5 AND birthday_day >= 21) OR (birthday_month = 6 AND birthday_day <= 20) THEN 'Gemini'
    WHEN (birthday_month = 6 AND birthday_day >= 21) OR (birthday_month = 7 AND birthday_day <= 22) THEN 'Cancer'
    WHEN (birthday_month = 7 AND birthday_day >= 23) OR (birthday_month = 8 AND birthday_day <= 22) THEN 'Leo'
    WHEN (birthday_month = 8 AND birthday_day >= 23) OR (birthday_month = 9 AND birthday_day <= 22) THEN 'Virgo'
    WHEN (birthday_month = 9 AND birthday_day >= 23) OR (birthday_month = 10 AND birthday_day <= 22) THEN 'Libra'
    WHEN (birthday_month = 10 AND birthday_day >= 23) OR (birthday_month = 11 AND birthday_day <= 21) THEN 'Scorpio'
    WHEN (birthday_month = 11 AND birthday_day >= 22) OR (birthday_month = 12 AND birthday_day <= 21) THEN 'Sagittarius'
    WHEN (birthday_month = 12 AND birthday_day >= 22) OR (birthday_month = 1 AND birthday_day <= 19) THEN 'Capricorn'
    WHEN (birthday_month = 1 AND birthday_day >= 20) OR (birthday_month = 2 AND birthday_day <= 18) THEN 'Aquarius'
    WHEN (birthday_month = 2 AND birthday_day >= 19) OR (birthday_month = 3 AND birthday_day <= 20) THEN 'Pisces'
    END
    FROM date_part('month', birthday) AS birthday_month, date_part('day', birthday) AS birthday_day
$$ IMMUTABLE LANGUAGE SQL;
