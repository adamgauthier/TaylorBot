ALTER SCHEMA reminders
    RENAME TO users;

CREATE TABLE users.cleverbot_sessions
(
    user_id text NOT NULL,
    session_created_at bigint NOT NULL,
    PRIMARY KEY (user_id),
    CONSTRAINT user_id_fk FOREIGN KEY (user_id)
        REFERENCES public.users (user_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)
WITH (
    OIDS = FALSE
);

ALTER TABLE users.cleverbot_sessions
    OWNER to postgres;

GRANT ALL ON TABLE users.cleverbot_sessions TO taylorbot;

ALTER TABLE public.guilds
  SET SCHEMA guilds;

ALTER TABLE public.guild_special_roles
  SET SCHEMA guilds;

ALTER TABLE public.guild_role_groups
  SET SCHEMA guilds;

ALTER TABLE public.guild_names
  SET SCHEMA guilds;

ALTER TABLE public.guild_commands
  SET SCHEMA guilds;