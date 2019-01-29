CREATE SCHEMA attributes
    AUTHORIZATION postgres;

GRANT USAGE ON SCHEMA attributes TO taylorbot;

CREATE TABLE attributes.attributes
(
    attribute_id text NOT NULL,
    created_at bigint NOT NULL,
    PRIMARY KEY (attribute_id)
)
WITH (
    OIDS = FALSE
);

ALTER TABLE attributes.attributes
    OWNER to postgres;

GRANT ALL ON TABLE attributes.attributes TO taylorbot;

CREATE TABLE attributes.text_attributes
(
    attribute_id text NOT NULL,
    user_id text NOT NULL,
    attribute_value text NOT NULL,
    PRIMARY KEY (attribute_id, user_id),
    CONSTRAINT attribute_id_fk FOREIGN KEY (attribute_id)
        REFERENCES attributes.attributes (attribute_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT user_id_fk FOREIGN KEY (user_id)
        REFERENCES users.users (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)
WITH (
    OIDS = FALSE
);

ALTER TABLE attributes.text_attributes
    OWNER to postgres;

GRANT ALL ON TABLE attributes.text_attributes TO taylorbot;