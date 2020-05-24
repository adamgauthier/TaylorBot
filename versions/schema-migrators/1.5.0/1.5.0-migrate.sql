ALTER TABLE checkers.instagram_checker
    ADD COLUMN last_taken_at timestamp with time zone NOT NULL DEFAULT TIMESTAMP WITH TIME ZONE '2000-01-01 00:00:00';

CREATE TABLE users.taypoint_wills
(
    owner_user_id text NOT NULL,
    beneficiary_user_id text NOT NULL,
    created_at timestamp with time zone NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (owner_user_id),
    CONSTRAINT owner_user_id_fk FOREIGN KEY (owner_user_id)
        REFERENCES users.users (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID,
    CONSTRAINT beneficiary_user_id_fk FOREIGN KEY (beneficiary_user_id)
        REFERENCES users.users (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
        NOT VALID
);

ALTER TABLE users.taypoint_wills OWNER to postgres;

GRANT ALL ON TABLE users.taypoint_wills TO taylorbot;
