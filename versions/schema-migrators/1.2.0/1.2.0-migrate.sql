CREATE TABLE users.pro_users
(
    user_id text NOT NULL,
    expires_at timestamp with time zone,
    subscription_count integer NOT NULL,
    added_at timestamp with time zone NOT NULL DEFAULT (NOW()),
    PRIMARY KEY (user_id)
);

ALTER TABLE users.pro_users OWNER to postgres;

GRANT ALL ON TABLE users.pro_users TO taylorbot;

CREATE TABLE guilds.pro_guilds
(
    guild_id text NOT NULL,
    pro_user_id text NOT NULL,
    added_at timestamp with time zone NOT NULL DEFAULT (NOW()),
    PRIMARY KEY (guild_id, pro_user_id),
    CONSTRAINT pro_user_id_fk FOREIGN KEY (pro_user_id)
        REFERENCES users.pro_users (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
);

ALTER TABLE guilds.pro_guilds OWNER to postgres;

GRANT ALL ON TABLE guilds.pro_guilds TO taylorbot;