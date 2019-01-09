ALTER TABLE guilds.guild_members
    RENAME taypoint_count TO experience;

ALTER TABLE users.users
    ADD COLUMN taypoint_count bigint NOT NULL DEFAULT 0;

CREATE TABLE users.rps_stats
(
    user_id text NOT NULL,
    rps_wins bigint NOT NULL,
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