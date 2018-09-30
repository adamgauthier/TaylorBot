CREATE TABLE attributes.integer_attributes
(
    attribute_id text NOT NULL,
    user_id text NOT NULL,
    integer_value integer NOT NULL,
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

ALTER TABLE attributes.integer_attributes
    OWNER to postgres;

GRANT ALL ON TABLE attributes.integer_attributes TO taylorbot;