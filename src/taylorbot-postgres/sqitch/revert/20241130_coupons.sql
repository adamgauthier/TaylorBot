-- Revert taylorbot-postgres:20241130_coupons from pg

BEGIN;

DROP TABLE users.redeemed_coupons;

DROP TABLE commands.coupons;

CREATE TABLE checkers.instagram_checker (
    instagram_username text NOT NULL,
    guild_id text NOT NULL,
    channel_id text NOT NULL,
    last_post_code text,
    last_taken_at timestamp with time zone DEFAULT '2000-01-01 00:00:00+00'::timestamp with time zone NOT NULL,
    PRIMARY KEY (instagram_username, guild_id, channel_id),
    CONSTRAINT guild_id_fk FOREIGN KEY (guild_id) REFERENCES guilds.guilds(guild_id)
);

UPDATE commands.messages_of_the_day SET message = 'Create a squad and for a thrilling **Taypoint Bank heist** with </heist play:1183612687935078512>! 💰'
WHERE message = 'Create a squad for a thrilling **Taypoint Bank heist** with </heist play:1183612687935078512>! 💰';

COMMIT;
