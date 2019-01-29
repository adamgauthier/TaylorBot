ALTER TABLE guilds.guild_members
    RENAME taypoint_count TO experience;

ALTER TABLE users.users
    ADD COLUMN taypoint_count bigint NOT NULL DEFAULT 0;

CREATE TABLE users.rps_stats
(
    user_id text NOT NULL,
    rps_win_count bigint NOT NULL,
    PRIMARY KEY (user_id),
    CONSTRAINT user_id_fk FOREIGN KEY (user_id)
        REFERENCES users.users (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)
WITH (
    OIDS = FALSE
);

ALTER TABLE users.rps_stats
    OWNER to postgres;

GRANT ALL ON TABLE users.rps_stats TO taylorbot;

ALTER TABLE commands.commands
    ADD COLUMN added_at timestamp with time zone NOT NULL DEFAULT (now());

ALTER TABLE commands.commands
    ADD COLUMN successful_use_count bigint NOT NULL DEFAULT 0;

ALTER TABLE commands.commands
    ADD COLUMN unhandled_error_count bigint NOT NULL DEFAULT 0;

CREATE TABLE users.roll_stats
(
    user_id text NOT NULL,
    roll_count bigint NOT NULL DEFAULT 0,
    perfect_roll_count bigint NOT NULL DEFAULT 0,
    PRIMARY KEY (user_id),
    CONSTRAINT user_id_fk FOREIGN KEY (user_id)
        REFERENCES users.users (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)
WITH (
    OIDS = FALSE
);

ALTER TABLE users.roll_stats
    OWNER to postgres;

GRANT ALL ON TABLE users.roll_stats TO taylorbot;

CREATE TABLE users.gamble_stats
(
    user_id text NOT NULL,
    gamble_win_count bigint NOT NULL DEFAULT 0,
    gamble_win_amount bigint NOT NULL DEFAULT 0,
    gamble_lose_count bigint NOT NULL DEFAULT 0,
    gamble_lose_amount bigint NOT NULL DEFAULT 0,
    PRIMARY KEY (user_id),
    CONSTRAINT user_id_fk FOREIGN KEY (user_id)
        REFERENCES users.users (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)
WITH (
    OIDS = FALSE
);

ALTER TABLE users.gamble_stats
    OWNER to postgres;

GRANT ALL ON TABLE users.gamble_stats TO taylorbot;

CREATE TABLE users.daily_payouts
(
    user_id text NOT NULL,
    last_payout_at timestamp with time zone NOT NULL DEFAULT (now()),
    PRIMARY KEY (user_id),
    CONSTRAINT user_id_fk FOREIGN KEY (user_id)
        REFERENCES users.users (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)
WITH (
    OIDS = FALSE
);

ALTER TABLE users.daily_payouts
    OWNER to postgres;

GRANT ALL ON TABLE users.daily_payouts TO taylorbot;