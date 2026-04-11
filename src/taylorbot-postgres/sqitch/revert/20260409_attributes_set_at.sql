-- Revert taylorbot-postgres:20260409_attributes_set_at from pg

BEGIN;

-- Remove set_at columns
ALTER TABLE attributes.birthdays DROP COLUMN set_at;
ALTER TABLE attributes.location_attributes DROP COLUMN set_at;
ALTER TABLE attributes.integer_attributes DROP COLUMN set_at;
ALTER TABLE attributes.text_attributes DROP COLUMN set_at;

-- Recreate the parent attributes table
CREATE TABLE attributes.attributes (
    attribute_id text NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (attribute_id)
);

-- Re-insert all known attribute IDs
INSERT INTO attributes.attributes (attribute_id) VALUES
    ('age'),
    ('bae'),
    ('birthday'),
    ('dailypayoutstreak'),
    ('favoritesongs'),
    ('gamblefails'),
    ('gamblelosses'),
    ('gambleprofits'),
    ('gamblewins'),
    ('gender'),
    ('heistfails'),
    ('heistlosses'),
    ('heistprofits'),
    ('heistwins'),
    ('instagram'),
    ('joined'),
    ('lastfm'),
    ('location'),
    ('messages'),
    ('minutes'),
    ('oldminutes'),
    ('perfectrolls'),
    ('rolls'),
    ('rpswins'),
    ('snapchat'),
    ('taypoints'),
    ('tumblr'),
    ('waifu'),
    ('words')
ON CONFLICT DO NOTHING;

-- Re-add FK constraints
ALTER TABLE attributes.text_attributes
    ADD CONSTRAINT attribute_id_fk FOREIGN KEY (attribute_id) REFERENCES attributes.attributes(attribute_id);
ALTER TABLE attributes.integer_attributes
    ADD CONSTRAINT attribute_id_fk FOREIGN KEY (attribute_id) REFERENCES attributes.attributes(attribute_id);

COMMIT;
