CREATE SCHEMA egghunt2025;

CREATE TABLE egghunt2025.eggs (
    egg_number text UNIQUE NOT NULL,
    egg_code text NOT NULL,
    PRIMARY KEY (egg_number)
);

CREATE TABLE egghunt2025.egg_finds (
    egg_number text NOT NULL,
    user_id text NOT NULL,
    username text NOT NULL,
    found_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (egg_number, user_id),
    CONSTRAINT egg_number_fk FOREIGN KEY (egg_number) REFERENCES egghunt2025.eggs(egg_number)
);

INSERT INTO egghunt2025.eggs (egg_number, egg_code)
VALUES
 ('01', 'WHDQDJD34ZCXKVPL')
,('02', 'P39E5AMFCK945FVS')
,('03', 'CQKUCBALRUC47CNE')
,('04', 'P3JMDWZUKUCTMWEC')
,('05', '49JX4QCD7243LEJE')
,('06', 'J3VHFM5NUL4JNV3Y')
,('07', 'ZLY2FFDWXZKJ8RME')
,('08', 'L9X7Z25PFLCKRCJ9')
,('09', 'UQL75FTRAX4CS3U9')
,('10', 'CE927EA5CV4FLWM9')
,('11', 'X2257FXA2Z9T54RS')
,('12', 'TMCC9F9D95AF2C2C')
,('13', '3784292402085232')
;

CREATE TABLE egghunt2025.config (
    config_key text NOT NULL,
    config_value text NOT NULL,
    PRIMARY KEY (config_key)
);

-- All egg finds, newest first
SELECT * FROM egghunt2025.egg_finds
INNER JOIN egghunt2025.eggs ON egghunt2025.eggs.egg_number = egghunt2025.egg_finds.egg_number
ORDER BY found_at DESC;

-- All egg finds for a specific user
SELECT * FROM egghunt2025.egg_finds
INNER JOIN egghunt2025.eggs ON egghunt2025.eggs.egg_number = egghunt2025.egg_finds.egg_number
WHERE user_id = '1'
ORDER BY found_at DESC;

-- Total number of players and egg finds
SELECT COUNT(*) AS total_egg_finds, COUNT(DISTINCT user_id) AS total_users
FROM egghunt2025.egg_finds;

-- All eggs and who solved them first
WITH TEMP AS
(
    SELECT
        egghunt2025.eggs.egg_number,
        egg_code,
        user_id,
        username,
        found_at,
        rank() OVER (PARTITION BY egghunt2025.egg_finds.egg_number ORDER BY found_at) AS RK
    FROM egghunt2025.egg_finds INNER JOIN egghunt2025.eggs ON egghunt2025.eggs.egg_number = egghunt2025.egg_finds.egg_number
)
SELECT egg_code, egg_number, user_id, username, found_at
FROM TEMP WHERE RK=1
ORDER BY found_at;

-- Users with at least 11 egg finds
SELECT
    user_id,
    username,
    COUNT(*) AS egg_find_count
FROM egghunt2025.egg_finds
GROUP BY user_id, username
HAVING COUNT(*) >= 11
ORDER BY egg_find_count DESC;

-- People who found a specific egg in order
SELECT * FROM egghunt2025.egg_finds
INNER JOIN egghunt2025.eggs ON egghunt2025.egg_finds.egg_number = egghunt2025.eggs.egg_number
WHERE eggs.egg_number = '01'
ORDER BY found_at;

-- Reward all egg finds (aim for 25k reward sum)
WITH total AS (
    SELECT COUNT(DISTINCT user_id) AS total_users
    FROM egghunt2025.egg_finds
)
-- 40 taypoints per person who didn't find that egg
SELECT
    found_counts.*,
    ((SELECT COUNT(DISTINCT user_id) AS total_users FROM egghunt2025.egg_finds) - found_counts.found_by) * 40 AS reward
FROM (
    SELECT e.egg_number, COUNT(ef.egg_number) AS found_by
    FROM egghunt2025.eggs e
    LEFT JOIN egghunt2025.egg_finds ef ON e.egg_number = ef.egg_number
    GROUP BY e.egg_number
    ORDER BY found_by DESC, egg_number ASC
) found_counts;

CREATE OR REPLACE FUNCTION pg_temp.reward_users_for_egg_find(egg_num text, reward_points integer)
RETURNS TABLE(_user_id text) AS $$
BEGIN
    RETURN QUERY
    UPDATE users.users
    SET taypoint_count = taypoint_count + reward_points
    WHERE users.user_id IN (
        SELECT egg_finds.user_id FROM egghunt2025.egg_finds
        INNER JOIN egghunt2025.eggs ON egghunt2025.egg_finds.egg_number = egghunt2025.eggs.egg_number
        WHERE eggs.egg_number = egg_num
    )
    RETURNING user_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION pg_temp.reward_users_for_egg(egg_num text, reward_points integer)
RETURNS TABLE(_reward_message text) AS $$
BEGIN
    RETURN QUERY
    SELECT CONCAT('You just got **', reward_points::text, ' taypoints** for finding egg **#', egg_num, '** first! Congrats 🐰🥚 <@', string_agg(_user_id, '> <@'), '>')
    FROM pg_temp.reward_users_for_egg_find(egg_num, reward_points);
END;
$$ LANGUAGE plpgsql;


-- Reward first to find eggs
WITH first_finds AS (
    SELECT
        *,
        ROW_NUMBER() OVER (PARTITION BY egg_number ORDER BY found_at) AS row_num
    FROM
        egghunt2025.egg_finds
)
SELECT
    *
FROM
    first_finds
WHERE
    row_num = CASE
        -- Custom exclusions for bot owners
        WHEN egg_number = '06' THEN 2
        ELSE CASE
            WHEN egg_number = '05' THEN 5
            ELSE CASE
                WHEN egg_number = '07' THEN 2
                ELSE CASE WHEN egg_number = '13' THEN 2 ELSE 1 END
            END
        END
    END
ORDER BY found_at;
