CREATE SCHEMA egghunt2023;

CREATE TABLE egghunt2023.eggs (
    egg_id text NOT NULL,
    egg_number bigint UNIQUE NOT NULL,
    PRIMARY KEY (egg_id)
);

CREATE TABLE egghunt2023.egg_finds (
    egg_id text NOT NULL,
    user_id text NOT NULL,
    username text NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (egg_id, user_id),
    CONSTRAINT egg_id_fk FOREIGN KEY (egg_id) REFERENCES egghunt2023.eggs(egg_id)
);

INSERT INTO egghunt2023.eggs (egg_id, egg_number) VALUES ('sqxfdxV8cnFsQZqv', 1);
INSERT INTO egghunt2023.eggs (egg_id, egg_number) VALUES ('EGmVAUfaefWxUSq4', 2);
INSERT INTO egghunt2023.eggs (egg_id, egg_number) VALUES ('KN8acsn26xVACtme', 3);
INSERT INTO egghunt2023.eggs (egg_id, egg_number) VALUES ('7TXG8gWwcBVgPVVs', 4);
INSERT INTO egghunt2023.eggs (egg_id, egg_number) VALUES ('ytf9hA7ZdspXj1BE', 5);
INSERT INTO egghunt2023.eggs (egg_id, egg_number) VALUES ('EGJpEfvf2TWWesCZ', 6);
INSERT INTO egghunt2023.eggs (egg_id, egg_number) VALUES ('c9CsVgXwgpavskxE', 7);
INSERT INTO egghunt2023.eggs (egg_id, egg_number) VALUES ('GRAJUwp3epqK9hJH', 8);
INSERT INTO egghunt2023.eggs (egg_id, egg_number) VALUES ('QKxEt7jBNvfpzpvW', 9);
INSERT INTO egghunt2023.eggs (egg_id, egg_number) VALUES ('jfGBjcAsCEgb4Zj3', 10);
INSERT INTO egghunt2023.eggs (egg_id, egg_number) VALUES ('SeCTMbVbywhx7rTP', 11);
INSERT INTO egghunt2023.eggs (egg_id, egg_number) VALUES ('7AhwUvsKGS1fmpaW', 12);
INSERT INTO egghunt2023.eggs (egg_id, egg_number) VALUES ('DJEn7tKRgnaTsZtp', 13);

CREATE TABLE egghunt2023.config (
    config_key text NOT NULL,
    config_value text NOT NULL,
    PRIMARY KEY (config_key)
);


SELECT * FROM egghunt2023.egg_finds INNER JOIN egghunt2023.eggs ON egghunt2023.eggs.egg_id = egghunt2023.egg_finds.egg_id ORDER BY created_at;

SELECT COUNT(*) AS total_egg_finds, COUNT(DISTINCT user_id) AS total_users FROM egghunt2023.egg_finds;

WITH TEMP AS
(
SELECT egghunt2023.eggs.egg_id, egg_number, user_id, username, created_at,
rank() OVER (PARTITION BY egghunt2023.egg_finds.egg_id ORDER BY created_at) AS RK
FROM egghunt2023.egg_finds INNER JOIN egghunt2023.eggs ON egghunt2023.eggs.egg_id = egghunt2023.egg_finds.egg_id
)
SELECT egg_number, egg_id, user_id, username, created_at as found_at FROM TEMP WHERE RK=1 ORDER BY created_at;

SELECT user_id, username, cnt AS eggs_found
FROM (
   SELECT user_id, username, cnt,
          RANK() OVER (PARTITION BY user_id
                       ORDER BY cnt DESC) AS rn
   FROM (
      SELECT user_id, username, COUNT(user_id) AS cnt
      FROM egghunt2023.egg_finds
      GROUP BY user_id, username) t
) s
WHERE s.rn = 1
ORDER BY cnt DESC;

SELECT * FROM egghunt2023.egg_finds INNER JOIN egghunt2023.eggs ON egghunt2023.egg_finds.egg_id = egghunt2023.eggs.egg_id WHERE egg_number = 1;

SELECT s.egg_id, egg_number, cnt AS found_by
FROM (
   SELECT egg_id, cnt,
          RANK() OVER (PARTITION BY egg_id
                       ORDER BY cnt DESC) AS rn
   FROM (
      SELECT egg_id, COUNT(egg_id) AS cnt
      FROM egghunt2023.egg_finds
      GROUP BY egg_id) t
) s INNER JOIN egghunt2023.eggs ON egghunt2023.eggs.egg_id = s.egg_id
WHERE s.rn = 1
ORDER BY cnt DESC;
