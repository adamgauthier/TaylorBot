CREATE SCHEMA valentines2026;

CREATE TABLE valentines2026.role_obtained (
    user_id text NOT NULL,
    username text NOT NULL,
    acquired_from_user_id text NOT NULL,
    acquired_from_username text NOT NULL,
    acquired_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (user_id)
);

CREATE TABLE valentines2026.config (
    config_key text NOT NULL,
    config_value text NOT NULL,
    PRIMARY KEY (config_key)
);

INSERT INTO valentines2026.config (config_key, config_value) VALUES
('spread_love_role_id', 'REPLACE_WITH_NEW_ROLE_ID'),
('incubation_period', '0.02:00:00'),
('bypass_spread_limit_role_ids', '827370245667946526'),
('spread_limit', '1'),
('lounge_channel_id', '587420269191364648'),
('giveaways_end_time', '2026-02-16T07:59:59.0000000Z'),
('timespan_between_giveaways', '0.00:30:00'),
('giveaway_prize_min', '100'),
('giveaway_prize_max', '501');

INSERT INTO valentines2026.role_obtained (user_id, username, acquired_from_user_id, acquired_from_username)
VALUES ('119341483219353602', 'adam', '119341483219353602', 'adam');
