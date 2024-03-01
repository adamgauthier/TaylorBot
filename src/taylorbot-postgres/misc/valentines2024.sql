CREATE SCHEMA valentines2024;

CREATE TABLE valentines2024.codes (
    puzzle_id text NOT NULL,
    puzzle_code text NOT NULL,
    enabled boolean DEFAULT FALSE NOT NULL,
    PRIMARY KEY (puzzle_id)
);

INSERT INTO valentines2024.codes (puzzle_id, puzzle_code)
VALUES
('fearless', '1'),
('speaknow', '2'),
('red', '3'),
('1989', '4');

CREATE TABLE valentines2024.puzzle_solves (
    puzzle_id text NOT NULL,
    user_id text NOT NULL,
    username text NOT NULL,
    solved_at timestamp with time zone,
    attempt_count smallint,
    first_attempt_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (puzzle_id, user_id),
    CONSTRAINT puzzle_id_fk FOREIGN KEY (puzzle_id) REFERENCES valentines2024.codes(puzzle_id)
);

CREATE TABLE valentines2024.config (
    config_key text NOT NULL,
    config_value text NOT NULL,
    PRIMARY KEY (config_key)
);

-- All puzzle solves in order
SELECT * FROM valentines2024.puzzle_solves
INNER JOIN valentines2024.codes ON valentines2024.codes.puzzle_id = valentines2024.puzzle_solves.puzzle_id
WHERE solved_at IS NOT NULL
ORDER BY solved_at ASC;

-- Total number of players and times solved
SELECT SUM(attempt_count) AS total_attempts, COUNT(*) AS total_puzzle_solves, COUNT(DISTINCT user_id) AS total_users
FROM valentines2024.puzzle_solves
WHERE solved_at IS NOT NULL;

-- All puzzles and who solved them first
WITH TEMP AS
(
    SELECT valentines2024.codes.puzzle_id, puzzle_code, user_id, username, solved_at,
    rank() OVER (PARTITION BY valentines2024.puzzle_solves.puzzle_id ORDER BY solved_at) AS RK
    FROM valentines2024.puzzle_solves INNER JOIN valentines2024.codes ON valentines2024.codes.puzzle_id = valentines2024.puzzle_solves.puzzle_id
    WHERE solved_at IS NOT NULL
)
SELECT puzzle_code, puzzle_id, user_id, username, solved_at as found_at FROM TEMP WHERE RK=1 ORDER BY solved_at;

-- Number of puzzles each user solved
SELECT user_id, username, cnt AS puzzles_solved
FROM (
    SELECT
        user_id,
        username,
        cnt,
        RANK() OVER (PARTITION BY user_id ORDER BY cnt DESC) AS rn
    FROM (
        SELECT user_id, username, COUNT(user_id) AS cnt
        FROM valentines2024.puzzle_solves
        WHERE solved_at IS NOT NULL
        GROUP BY user_id, username) t
) s
WHERE s.rn = 1
ORDER BY cnt DESC;

-- People who solved a specific puzzle in order
SELECT * FROM valentines2024.puzzle_solves
INNER JOIN valentines2024.codes ON valentines2024.puzzle_solves.puzzle_id = valentines2024.codes.puzzle_id
WHERE solved_at IS NOT NULL AND codes.puzzle_id = 'fearless'
ORDER BY solved_at ASC;

-- How many people solved each puzzle
SELECT s.puzzle_id, cnt AS solved_by
FROM (
    SELECT
        puzzle_id,
        cnt,
        RANK() OVER (PARTITION BY puzzle_id ORDER BY cnt DESC) AS rn
    FROM (
        SELECT puzzle_id, COUNT(puzzle_id) AS cnt
        FROM valentines2024.puzzle_solves
        WHERE solved_at IS NOT NULL
        GROUP BY puzzle_id) t
) s INNER JOIN valentines2024.codes ON valentines2024.codes.puzzle_id = s.puzzle_id
WHERE s.rn = 1
ORDER BY cnt DESC;
