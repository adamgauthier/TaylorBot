CREATE SCHEMA valentines2023;

CREATE TABLE valentines2023.role_obtained (
    user_id text NOT NULL,
    full_username text NOT NULL,
    acquired_from_user_id text NOT NULL,
    acquired_from_full_username text NOT NULL,
    acquired_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (user_id)
);

CREATE TABLE valentines2023.config (
    config_key text NOT NULL,
    config_value text NOT NULL,
    PRIMARY KEY (config_key)
);

INSERT INTO valentines2023.config (config_key, config_value) VALUES
('spread_love_role_id', '640217114875265047'),
('incubation_period', '0.00:05:00'),
('bypass_spread_limit_role_ids', '115334158892531719'),
('spread_limit', '2')
('lounge_channel_id', '587420269191364648')
('giveaways_end_time', '2023-02-18T07:59:59.0000000Z')
('timespan_between_giveaways', '0.00:02:00')
('giveaway_prize_min', '100')
('giveaway_prize_max', '501');

INSERT INTO valentines2023.role_obtained (user_id, full_username, acquired_from_user_id, acquired_from_full_username, acquired_at)
VALUES ('119341483219353602', 'Adam#0420', '119341483219353602', 'Adam#0420');
