ALTER TABLE checkers.youtube_checker
    ADD CONSTRAINT guild_id_fk FOREIGN KEY (guild_id)
    REFERENCES public.guilds (guild_id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;

ALTER TABLE checkers.tumblr_checker
    ADD CONSTRAINT guild_id_fk FOREIGN KEY (guild_id)
    REFERENCES public.guilds (guild_id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;

ALTER TABLE checkers.reddit_checker
    ADD CONSTRAINT guild_id_fk FOREIGN KEY (guild_id)
    REFERENCES public.guilds (guild_id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;

ALTER TABLE checkers.instagram_checker
    ADD CONSTRAINT guild_id_fk FOREIGN KEY (guild_id)
    REFERENCES public.guilds (guild_id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION;

CREATE TABLE public.guild_names
(
    guild_id text NOT NULL,
    guild_name text NOT NULL,
    changed_at bigint NOT NULL,
    PRIMARY KEY (guild_id, changed_at),
    CONSTRAINT guild_id_fk FOREIGN KEY (guild_id)
        REFERENCES public.guilds (guild_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)
WITH (
    OIDS = FALSE
);

ALTER TABLE public.guild_names
    OWNER to postgres;

GRANT ALL ON TABLE public.guild_names TO taylorbot;

GRANT SELECT ON TABLE public.guild_names TO remote_access;

CREATE TABLE public.guild_special_roles
(
    guild_id text NOT NULL,
    role_id text NOT NULL,
    accessible boolean NOT NULL DEFAULT FALSE,
    PRIMARY KEY (role_id, guild_id),
    CONSTRAINT guild_id_fk FOREIGN KEY (guild_id)
        REFERENCES public.guilds (guild_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)
WITH (
    OIDS = FALSE
);

ALTER TABLE public.guild_special_roles
    OWNER to postgres;

GRANT ALL ON TABLE public.guild_special_roles TO taylorbot;

GRANT SELECT ON TABLE public.guild_special_roles TO remote_access;