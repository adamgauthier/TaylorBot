CREATE TABLE guilds.channel_commands
(
    guild_id text NOT NULL,
    channel_id text NOT NULL,
    command_id text NOT NULL,
    PRIMARY KEY (guild_id, channel_id, command_id),
    CONSTRAINT channel_id_fk FOREIGN KEY (guild_id, channel_id)
        REFERENCES guilds.text_channels (guild_id, channel_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION,
    CONSTRAINT command_id_fk FOREIGN KEY (command_id)
        REFERENCES commands.commands (name) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION
)
WITH (
    OIDS = FALSE
);

ALTER TABLE guilds.channel_commands
    OWNER to postgres;

GRANT ALL ON TABLE guilds.channel_commands TO taylorbot;