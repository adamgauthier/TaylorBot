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