-- Deploy taylorbot-postgres:20241130_coupons to pg

BEGIN;

UPDATE commands.messages_of_the_day SET message = 'Create a squad for a thrilling **Taypoint Bank heist** with </heist play:1183612687935078512>! 💰'
WHERE message = 'Create a squad and for a thrilling **Taypoint Bank heist** with </heist play:1183612687935078512>! 💰';

DROP TABLE checkers.instagram_checker;

CREATE TABLE commands.coupons (
    coupon_id uuid DEFAULT public.gen_random_uuid() NOT NULL,
    code text UNIQUE NOT NULL,
    added_at timestamp with time zone DEFAULT now() NOT NULL,
    valid_from timestamp with time zone NOT NULL,
    valid_until timestamp with time zone NOT NULL,
    usage_limit integer,
    used_count integer DEFAULT 0 NOT NULL,
    taypoint_reward bigint NOT NULL,
    PRIMARY KEY (coupon_id)
);

CREATE TABLE users.redeemed_coupons (
    user_id text NOT NULL,
    coupon_id uuid NOT NULL,
    coupon_code text NOT NULL,
    coupon_reward bigint NOT NULL,
    redeemed_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (user_id, coupon_id)
);

COMMIT;
