CREATE SCHEMA egghunt2026;

CREATE TABLE egghunt2026.eggs (
    egg_number text UNIQUE NOT NULL,
    egg_code text NOT NULL,
    PRIMARY KEY (egg_number)
);

CREATE TABLE egghunt2026.egg_finds (
    egg_number text NOT NULL,
    user_id text NOT NULL,
    username text NOT NULL,
    found_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (egg_number, user_id),
    CONSTRAINT egg_number_fk FOREIGN KEY (egg_number) REFERENCES egghunt2026.eggs(egg_number)
);

INSERT INTO egghunt2026.eggs (egg_number, egg_code)
VALUES
 ('01', '3KX2VDE7YTAESKHK')
,('02', '7V79P4ZPAVXJC9ZT')
,('03', 'L92HEHA24SFPALQF')
,('04', 'ANFPMDQFPUQFZVYL')
,('05', 'DYEDKEYLIMEGREEN')
,('06', 'NVPL32WRWPXEW9YN')
,('07', 'SEQHZPPWHJWVAPSE')
,('08', 'EKAK4R34TU3JKW23')
,('09', 'NMQWCZH5YFUQS3M5')
,('10', '2RTPSDC3ENAUACYZ')
,('11', 'E2AYCUQLH97FTH54')
,('12', '1520C755BDC60025')
,('13', 'WVLLKQLASEA77A4T')
;

CREATE TABLE egghunt2026.config (
    config_key text NOT NULL,
    config_value text NOT NULL,
    PRIMARY KEY (config_key)
);

-- All egg finds, newest first
SELECT * FROM egghunt2026.egg_finds
INNER JOIN egghunt2026.eggs ON egghunt2026.eggs.egg_number = egghunt2026.egg_finds.egg_number
ORDER BY found_at DESC;

-- All egg finds for a specific user
SELECT * FROM egghunt2026.egg_finds
INNER JOIN egghunt2026.eggs ON egghunt2026.eggs.egg_number = egghunt2026.egg_finds.egg_number
WHERE user_id = '1'
ORDER BY found_at DESC;

-- Total number of players and egg finds
SELECT COUNT(*) AS total_egg_finds, COUNT(DISTINCT user_id) AS total_users
FROM egghunt2026.egg_finds;

-- All eggs and who solved them first
WITH TEMP AS
(
    SELECT
        egghunt2026.eggs.egg_number,
        egg_code,
        user_id,
        username,
        found_at,
        rank() OVER (PARTITION BY egghunt2026.egg_finds.egg_number ORDER BY found_at) AS RK
    FROM egghunt2026.egg_finds INNER JOIN egghunt2026.eggs ON egghunt2026.eggs.egg_number = egghunt2026.egg_finds.egg_number
)
SELECT egg_code, egg_number, user_id, username, found_at
FROM TEMP WHERE RK=1
ORDER BY found_at;

-- Users with at least 11 egg finds
SELECT
    user_id,
    username,
    COUNT(*) AS egg_find_count
FROM egghunt2026.egg_finds
GROUP BY user_id, username
HAVING COUNT(*) >= 11
ORDER BY egg_find_count DESC;

-- People who found a specific egg in order
SELECT * FROM egghunt2026.egg_finds
INNER JOIN egghunt2026.eggs ON egghunt2026.egg_finds.egg_number = egghunt2026.eggs.egg_number
WHERE eggs.egg_number = '01'
ORDER BY found_at;

-- Reward all egg finds (46 taypoints per person who didn't find that egg)
SELECT
    found_counts.*,
    ((SELECT COUNT(DISTINCT user_id) AS total_users FROM egghunt2026.egg_finds) - found_counts.found_by) * 46 AS reward
FROM (
    SELECT e.egg_number, COUNT(ef.egg_number) AS found_by
    FROM egghunt2026.eggs e
    LEFT JOIN egghunt2026.egg_finds ef ON e.egg_number = ef.egg_number
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
        SELECT egg_finds.user_id FROM egghunt2026.egg_finds
        INNER JOIN egghunt2026.eggs ON egghunt2026.egg_finds.egg_number = egghunt2026.eggs.egg_number
        WHERE eggs.egg_number = egg_num
    )
    RETURNING user_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION pg_temp.reward_users_for_egg(egg_num text, reward_points integer)
RETURNS TABLE(_reward_message text) AS $$
BEGIN
    RETURN QUERY
    SELECT CONCAT('You just got **', reward_points::text, ' taypoints** for finding egg **#', egg_num, '**! Congrats 🐰🥚 <@', string_agg(_user_id, '> <@'), '>')
    FROM pg_temp.reward_users_for_egg_find(egg_num, reward_points);
END;
$$ LANGUAGE plpgsql;

-- First finders with exclusions (organizers/bot owners)
WITH filtered AS (
    SELECT *
    FROM egghunt2026.egg_finds
    WHERE NOT (egg_number = '02' AND user_id IN ('105861141024002048', '597436979973455882', '1149450067376357437'))
      AND NOT (egg_number = '03' AND user_id IN ('115329261350420487', '205784233011183616', '785421356417548301', '148132913211244545'))
      AND NOT (egg_number = '09' AND user_id IN ('105861141024002048', '597436979973455882', '1149450067376357437'))
),
ranked AS (
    SELECT *, ROW_NUMBER() OVER (PARTITION BY egg_number ORDER BY found_at) AS row_num
    FROM filtered
)
SELECT egg_number, user_id, username, found_at FROM ranked WHERE row_num = 1 ORDER BY found_at;
