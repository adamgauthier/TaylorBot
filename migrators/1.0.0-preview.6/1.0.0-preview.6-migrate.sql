CREATE EXTENSION pgcrypto;

CREATE TABLE public.reminders
(
    reminder_id uuid NOT NULL DEFAULT gen_random_uuid(),
    user_id text NOT NULL,
    created_at bigint NOT NULL,
    remind_at bigint NOT NULL,
    reminder_text text NOT NULL,
    PRIMARY KEY (reminder_id),
    CONSTRAINT user_id_fk FOREIGN KEY (user_id)
        REFERENCES public.users (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)
WITH (
    OIDS = FALSE
);

ALTER TABLE public.reminders
    OWNER to postgres;

CREATE SCHEMA reminders
    AUTHORIZATION postgres;

GRANT ALL ON SCHEMA reminders TO taylorbot;

ALTER TABLE public.reminders
  SET SCHEMA reminders;

GRANT ALL ON TABLE reminders.reminders TO taylorbot;

CREATE SCHEMA guilds
    AUTHORIZATION postgres;

GRANT ALL ON SCHEMA guilds TO taylorbot;

CREATE TABLE guilds.text_channels
(
    guild_id text NOT NULL,
    channel_id text NOT NULL,
    messages_count bigint NOT NULL DEFAULT 0,
    is_logging boolean NOT NULL DEFAULT FALSE,
    PRIMARY KEY (guild_id, channel_id),
    CONSTRAINT guild_id_fk FOREIGN KEY (guild_id)
        REFERENCES public.guilds (guild_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)
WITH (
    OIDS = FALSE
);

ALTER TABLE guilds.text_channels
    OWNER to postgres;

GRANT ALL ON TABLE guilds.text_channels TO taylorbot;