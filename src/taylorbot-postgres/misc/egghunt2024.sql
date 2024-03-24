CREATE SCHEMA egghunt2024;

CREATE TABLE egghunt2024.eggs (
    egg_number text UNIQUE NOT NULL,
    egg_code text NOT NULL,
    PRIMARY KEY (egg_number)
);

CREATE TABLE egghunt2024.egg_finds (
    egg_number text NOT NULL,
    user_id text NOT NULL,
    username text NOT NULL,
    found_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (egg_number, user_id),
    CONSTRAINT egg_number_fk FOREIGN KEY (egg_number) REFERENCES egghunt2024.eggs(egg_number)
);

INSERT INTO egghunt2024.eggs (egg_code, egg_number)
VALUES
 ('ABCDEFGH12345678', '01')
,('ABCDEFGH12345678', '02')
,('ABCDEFGH12345678', '03')
,('ABCDEFGH12345678', '04')
,('ABCDEFGH12345678', '05')
,('ABCDEFGH12345678', '06')
,('ABCDEFGH12345678', '07')
,('ABCDEFGH12345678', '08')
,('ABCDEFGH12345678', '09')
,('ABCDEFGH12345678', '10')
,('ABCDEFGH12345678', '11')
,('ABCDEFGH12345678', '12')
,('ABCDEFGH12345678', '13')
;

CREATE TABLE egghunt2024.config (
    config_key text NOT NULL,
    config_value text NOT NULL,
    PRIMARY KEY (config_key)
);


SELECT * FROM egghunt2024.egg_finds
INNER JOIN egghunt2024.eggs ON egghunt2024.eggs.egg_number = egghunt2024.egg_finds.egg_number
ORDER BY found_at;

SELECT COUNT(*) AS total_egg_finds, COUNT(DISTINCT user_id) AS total_users
FROM egghunt2024.egg_finds;

WITH TEMP AS
(
    SELECT
        egghunt2024.eggs.egg_number,
        egg_code,
        user_id,
        username,
        found_at,
        rank() OVER (PARTITION BY egghunt2024.egg_finds.egg_number ORDER BY found_at) AS RK
    FROM egghunt2024.egg_finds INNER JOIN egghunt2024.eggs ON egghunt2024.eggs.egg_number = egghunt2024.egg_finds.egg_number
)
SELECT egg_code, egg_number, user_id, username, found_at
FROM TEMP WHERE RK=1
ORDER BY found_at;

SELECT user_id, username, cnt AS eggs_found
FROM (
    SELECT
        user_id,
        username,
        cnt,
        RANK() OVER (PARTITION BY user_id ORDER BY cnt DESC) AS rn
    FROM (
        SELECT user_id, username, COUNT(user_id) AS cnt
        FROM egghunt2024.egg_finds
        GROUP BY user_id, username) t
) s
WHERE s.rn = 1
ORDER BY cnt DESC;

SELECT * FROM egghunt2024.egg_finds
INNER JOIN egghunt2024.eggs ON egghunt2024.egg_finds.egg_number = egghunt2024.eggs.egg_number
WHERE egg_number = '01';
