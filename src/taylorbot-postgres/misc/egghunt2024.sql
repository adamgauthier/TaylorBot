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
 ('DT544FAVA75QV9HV', '01')
,('4MT524XXCP5D7L47', '02')
,('WVHPFZC39L2QCTV3', '03')
,('YXT9AZVCSRU2SD9C', '04')
,('54EXJ4VRMDS72KZJ', '05')
,('SU3JDWDW3MLA5YAY', '06')
,('MA3N9RM9MWYXVZ2Y', '07')
,('66343M35R3V3RZ1P', '08')
,('92PE9KRDRN23LXM7', '09')
,('PDJT47YM97UW273S', '10')
,('FWQYPRLSYW3Y7ZN7', '11')
,('KNSRE377PYPNLJTT', '12')
,('CCMLUNCEJBDCACMJ', '13')
;

CREATE TABLE egghunt2024.config (
    config_key text NOT NULL,
    config_value text NOT NULL,
    PRIMARY KEY (config_key)
);

-- All egg finds in order
SELECT * FROM egghunt2024.egg_finds
INNER JOIN egghunt2024.eggs ON egghunt2024.eggs.egg_number = egghunt2024.egg_finds.egg_number
ORDER BY found_at;

-- Total number of players and egg finds
SELECT COUNT(*) AS total_egg_finds, COUNT(DISTINCT user_id) AS total_users
FROM egghunt2024.egg_finds;

-- All eggs and who solved them first
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

-- People who found a specific egg in order
SELECT * FROM egghunt2024.egg_finds
INNER JOIN egghunt2024.eggs ON egghunt2024.egg_finds.egg_number = egghunt2024.eggs.egg_number
WHERE eggs.egg_number = '01'
ORDER BY found_at;
