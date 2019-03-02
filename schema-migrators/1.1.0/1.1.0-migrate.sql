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
