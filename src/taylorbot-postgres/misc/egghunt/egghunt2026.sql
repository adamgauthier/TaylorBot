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
 ('01', 'AAAAAAAAAAAAAAAA')
,('02', 'BBBBBBBBBBBBBBBB')
,('03', 'CCCCCCCCCCCCCCCC')
,('04', 'DDDDDDDDDDDDDDDD')
,('05', 'EEEEEEEEEEEEEEEE')
,('06', 'FFFFFFFFFFFFFFFF')
,('07', 'GGGGGGGGGGGGGGGG')
,('08', 'HHHHHHHHHHHHHHHH')
,('09', 'IIIIIIIIIIIIIIII')
,('10', 'JJJJJJJJJJJJJJJJ')
,('11', 'KKKKKKKKKKKKKKKK')
,('12', 'LLLLLLLLLLLLLLLL')
,('13', 'MMMMMMMMMMMMMMMM')
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
