CREATE SCHEMA egghunt2022;

CREATE TABLE egghunt2022.eggs (
    egg_id text NOT NULL,
    egg_number bigint UNIQUE NOT NULL,
    PRIMARY KEY (egg_id)
);

CREATE TABLE egghunt2022.egg_finds (
    egg_id text NOT NULL,
    user_id text NOT NULL,
    username text NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (egg_id, user_id),
    CONSTRAINT egg_id_fk FOREIGN KEY (egg_id) REFERENCES egghunt2022.eggs(egg_id)
);

INSERT INTO egghunt2022.eggs (egg_id, egg_number) VALUES ('wEM8tu5xEC2XQTkV', 1);
INSERT INTO egghunt2022.eggs (egg_id, egg_number) VALUES ('3MQ2uPcHQyRsXY7Y', 2);
INSERT INTO egghunt2022.eggs (egg_id, egg_number) VALUES ('J8xHSKc9CKE9wuBB', 3);
INSERT INTO egghunt2022.eggs (egg_id, egg_number) VALUES ('cWevmfBsL4Ues4Q8', 4);
INSERT INTO egghunt2022.eggs (egg_id, egg_number) VALUES ('Gy6ZE25EAv2Hqyq4', 5);
INSERT INTO egghunt2022.eggs (egg_id, egg_number) VALUES ('NNRT7LDzEsDMrepP', 6);
INSERT INTO egghunt2022.eggs (egg_id, egg_number) VALUES ('gzd9c5t6T9Vda2VL', 7);
INSERT INTO egghunt2022.eggs (egg_id, egg_number) VALUES ('y7PQqq9RjCYrQt6w', 8);
INSERT INTO egghunt2022.eggs (egg_id, egg_number) VALUES ('4kUxq55fnnzEV9Xg', 9);
INSERT INTO egghunt2022.eggs (egg_id, egg_number) VALUES ('x7hqGvTh6HDPCPCq', 10);
INSERT INTO egghunt2022.eggs (egg_id, egg_number) VALUES ('txQL5pct4K4Rz8fG', 11);
INSERT INTO egghunt2022.eggs (egg_id, egg_number) VALUES ('jAT7ccmPLyyhbS9j', 12);
INSERT INTO egghunt2022.eggs (egg_id, egg_number) VALUES ('RBOPbW3xJec3hryp', 13);

CREATE TABLE egghunt2022.config (
    config_key text NOT NULL,
    config_value text NOT NULL,
    PRIMARY KEY (config_key)
);


SELECT * FROM egghunt2022.egg_finds INNER JOIN egghunt2022.eggs ON egghunt2022.eggs.egg_id = egghunt2022.egg_finds.egg_id ORDER BY created_at;

SELECT COUNT(*) AS total_egg_finds, COUNT(DISTINCT user_id) AS total_users FROM egghunt2022.egg_finds;

WITH TEMP AS
(
SELECT egghunt2022.eggs.egg_id, egg_number, user_id, username, created_at,
rank() OVER (PARTITION BY egghunt2022.egg_finds.egg_id ORDER BY created_at) AS RK
FROM egghunt2022.egg_finds INNER JOIN egghunt2022.eggs ON egghunt2022.eggs.egg_id = egghunt2022.egg_finds.egg_id
)
SELECT egg_number, egg_id, user_id, username, created_at as found_at FROM TEMP WHERE RK=1 ORDER BY created_at;

SELECT user_id, username, cnt AS eggs_found
FROM (
   SELECT user_id, username, cnt,
          RANK() OVER (PARTITION BY user_id
                       ORDER BY cnt DESC) AS rn
   FROM (
      SELECT user_id, username, COUNT(user_id) AS cnt
      FROM egghunt2022.egg_finds
      GROUP BY user_id, username) t
) s
WHERE s.rn = 1
ORDER BY cnt DESC;

SELECT * FROM egghunt2022.egg_finds INNER JOIN egghunt2022.eggs ON egghunt2022.egg_finds.egg_id = egghunt2022.eggs.egg_id WHERE egg_number = 1;

SELECT s.egg_id, egg_number, cnt AS found_by
FROM (
   SELECT egg_id, cnt,
          RANK() OVER (PARTITION BY egg_id
                       ORDER BY cnt DESC) AS rn
   FROM (
      SELECT egg_id, COUNT(egg_id) AS cnt
      FROM egghunt2022.egg_finds
      GROUP BY egg_id) t
) s INNER JOIN egghunt2022.eggs ON egghunt2022.eggs.egg_id = s.egg_id
WHERE s.rn = 1
ORDER BY cnt DESC;
