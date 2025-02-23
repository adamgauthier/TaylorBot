CREATE SCHEMA valentines2025;

CREATE TABLE valentines2025.role_obtained (
    user_id text NOT NULL,
    username text NOT NULL,
    acquired_from_user_id text NOT NULL,
    acquired_from_username text NOT NULL,
    acquired_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (user_id)
);

CREATE TABLE valentines2025.config (
    config_key text NOT NULL,
    config_value text NOT NULL,
    PRIMARY KEY (config_key)
);

INSERT INTO valentines2025.config (config_key, config_value) VALUES
('spread_love_role_id', '1338720368411807845'),
('incubation_period', '0.00:05:00'),
('bypass_spread_limit_role_ids', '827370245667946526'),
('spread_limit', '2'),
('lounge_channel_id', '587420269191364648'),
('giveaways_end_time', '2025-02-18T07:59:59.0000000Z'),
('timespan_between_giveaways', '0.00:02:00'),
('giveaway_prize_min', '100'),
('giveaway_prize_max', '501');

INSERT INTO valentines2025.role_obtained (user_id, username, acquired_from_user_id, acquired_from_username)
VALUES ('119341483219353602', 'adam', '119341483219353602', 'adam');
