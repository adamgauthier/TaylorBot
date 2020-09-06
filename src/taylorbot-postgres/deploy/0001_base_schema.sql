-- Deploy taylorbot-postgres:0001_base_schema to pg

BEGIN;

CREATE FUNCTION public._final_median(numeric[]) RETURNS numeric
    LANGUAGE sql IMMUTABLE
    AS $_$
   SELECT AVG(val)
   FROM (
     SELECT val
     FROM unnest($1) val
     ORDER BY 1
     LIMIT  2 - MOD(array_upper($1, 1), 2)
     OFFSET CEIL(array_upper($1, 1) / 2.0) - 1
   ) sub;
$_$;
CREATE AGGREGATE public.median(numeric) (
    SFUNC = array_append,
    STYPE = numeric[],
    INITCOND = '{}',
    FINALFUNC = public._final_median
);

CREATE FUNCTION public.zodiac(birthday date) RETURNS text
    LANGUAGE sql IMMUTABLE
    AS $$
    SELECT
    CASE  
    WHEN (birthday_month = 3 AND birthday_day >= 21) OR (birthday_month = 4 AND birthday_day <= 19) THEN 'Aries'
    WHEN (birthday_month = 4 AND birthday_day >= 20) OR (birthday_month = 5 AND birthday_day <= 20) THEN 'Taurus'
    WHEN (birthday_month = 5 AND birthday_day >= 21) OR (birthday_month = 6 AND birthday_day <= 20) THEN 'Gemini'
    WHEN (birthday_month = 6 AND birthday_day >= 21) OR (birthday_month = 7 AND birthday_day <= 22) THEN 'Cancer'
    WHEN (birthday_month = 7 AND birthday_day >= 23) OR (birthday_month = 8 AND birthday_day <= 22) THEN 'Leo'
    WHEN (birthday_month = 8 AND birthday_day >= 23) OR (birthday_month = 9 AND birthday_day <= 22) THEN 'Virgo'
    WHEN (birthday_month = 9 AND birthday_day >= 23) OR (birthday_month = 10 AND birthday_day <= 22) THEN 'Libra'
    WHEN (birthday_month = 10 AND birthday_day >= 23) OR (birthday_month = 11 AND birthday_day <= 21) THEN 'Scorpio'
    WHEN (birthday_month = 11 AND birthday_day >= 22) OR (birthday_month = 12 AND birthday_day <= 21) THEN 'Sagittarius'
    WHEN (birthday_month = 12 AND birthday_day >= 22) OR (birthday_month = 1 AND birthday_day <= 19) THEN 'Capricorn'
    WHEN (birthday_month = 1 AND birthday_day >= 20) OR (birthday_month = 2 AND birthday_day <= 18) THEN 'Aquarius'
    WHEN (birthday_month = 2 AND birthday_day >= 19) OR (birthday_month = 3 AND birthday_day <= 20) THEN 'Pisces'
    END
    FROM date_part('month', birthday) AS birthday_month, date_part('day', birthday) AS birthday_day
$$;


CREATE SCHEMA users;

CREATE TABLE users.users (
    user_id text NOT NULL,
    ignore_until timestamp with time zone DEFAULT now() NOT NULL,
    taypoint_count bigint DEFAULT 0 NOT NULL,
    is_bot boolean NOT NULL,
    username text NOT NULL,
    previous_username text,
    PRIMARY KEY (user_id)
);

CREATE TABLE users.cleverbot_sessions (
    user_id text NOT NULL,
    session_created_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (user_id),
    CONSTRAINT user_id_fk FOREIGN KEY (user_id) REFERENCES users.users(user_id)
);

CREATE TABLE users.daily_payouts (
    user_id text NOT NULL,
    last_payout_at timestamp with time zone DEFAULT now() NOT NULL,
    streak_count bigint DEFAULT 1 NOT NULL,
    PRIMARY KEY (user_id),
    CONSTRAINT user_id_fk FOREIGN KEY (user_id) REFERENCES users.users(user_id)
);

CREATE TABLE users.gamble_stats (
    user_id text NOT NULL,
    gamble_win_count bigint DEFAULT 0 NOT NULL,
    gamble_win_amount bigint DEFAULT 0 NOT NULL,
    gamble_lose_count bigint DEFAULT 0 NOT NULL,
    gamble_lose_amount bigint DEFAULT 0 NOT NULL,
    PRIMARY KEY (user_id),
    CONSTRAINT user_id_fk FOREIGN KEY (user_id) REFERENCES users.users(user_id)
);

CREATE TABLE users.heist_stats (
    user_id text NOT NULL,
    heist_win_count bigint DEFAULT 0 NOT NULL,
    heist_win_amount bigint DEFAULT 0 NOT NULL,
    heist_lose_count bigint DEFAULT 0 NOT NULL,
    heist_lose_amount bigint DEFAULT 0 NOT NULL,
    PRIMARY KEY (user_id),
    CONSTRAINT user_id_fk FOREIGN KEY (user_id) REFERENCES users.users(user_id)
);

CREATE TABLE users.pro_users (
    user_id text NOT NULL,
    expires_at timestamp with time zone,
    subscription_count integer NOT NULL,
    added_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (user_id)
);

CREATE TABLE users.reminders (
    reminder_id uuid DEFAULT public.gen_random_uuid() NOT NULL,
    user_id text NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    remind_at timestamp with time zone NOT NULL,
    reminder_text text NOT NULL,
    PRIMARY KEY (reminder_id),
    CONSTRAINT user_id_fk FOREIGN KEY (user_id) REFERENCES users.users(user_id)
);

CREATE TABLE users.roll_stats (
    user_id text NOT NULL,
    roll_count bigint DEFAULT 0 NOT NULL,
    perfect_roll_count bigint DEFAULT 0 NOT NULL,
    PRIMARY KEY (user_id),
    CONSTRAINT user_id_fk FOREIGN KEY (user_id) REFERENCES users.users(user_id)
);

CREATE TABLE users.rps_stats (
    user_id text NOT NULL,
    rps_win_count bigint NOT NULL,
    PRIMARY KEY (user_id),
    CONSTRAINT user_id_fk FOREIGN KEY (user_id) REFERENCES users.users(user_id)
);

CREATE TABLE users.taypoint_wills (
    owner_user_id text NOT NULL,
    beneficiary_user_id text NOT NULL,
    created_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    PRIMARY KEY (owner_user_id),
    CONSTRAINT beneficiary_user_id_fk FOREIGN KEY (beneficiary_user_id) REFERENCES users.users(user_id),
    CONSTRAINT owner_user_id_fk FOREIGN KEY (owner_user_id) REFERENCES users.users(user_id)
);

CREATE TABLE users.usernames (
    user_id text NOT NULL,
    username text NOT NULL,
    changed_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (user_id, changed_at),
    CONSTRAINT usernames_user_id_fkey FOREIGN KEY (user_id) REFERENCES users.users(user_id)
);


CREATE SCHEMA commands;

CREATE TABLE commands.commands (
    name text NOT NULL,
    enabled boolean DEFAULT true NOT NULL,
    added_at timestamp with time zone DEFAULT now() NOT NULL,
    successful_use_count bigint DEFAULT 0 NOT NULL,
    unhandled_error_count bigint DEFAULT 0 NOT NULL,
    aliases text[] NOT NULL,
    module_name text NOT NULL,
    PRIMARY KEY (name)
);

CREATE TABLE commands.messages_of_the_day (
    message text NOT NULL,
    added_at timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    priority_from timestamp with time zone,
    priority_to timestamp with time zone,
    PRIMARY KEY (message)
);

CREATE TABLE commands.user_groups (
    name text NOT NULL,
    access_level smallint NOT NULL,
    PRIMARY KEY (name)
);


CREATE SCHEMA guilds;

CREATE TABLE guilds.guilds (
    guild_id text NOT NULL,
    prefix text DEFAULT '!'::text NOT NULL,
    guild_name text NOT NULL,
    previous_guild_name text,
    PRIMARY KEY (guild_id)
);

CREATE TABLE guilds.text_channels (
    guild_id text NOT NULL,
    channel_id text NOT NULL,
    message_count bigint DEFAULT 0 NOT NULL,
    is_member_log boolean DEFAULT false NOT NULL,
    registered_at timestamp with time zone DEFAULT now() NOT NULL,
    is_spam boolean DEFAULT false NOT NULL,
    is_message_log boolean DEFAULT false NOT NULL,
    PRIMARY KEY (guild_id, channel_id),
    CONSTRAINT guild_id_fk FOREIGN KEY (guild_id) REFERENCES guilds.guilds(guild_id)
);

CREATE TABLE guilds.channel_commands (
    guild_id text NOT NULL,
    channel_id text NOT NULL,
    command_id text NOT NULL,
    PRIMARY KEY (guild_id, channel_id, command_id),
    CONSTRAINT channel_id_fk FOREIGN KEY (guild_id, channel_id) REFERENCES guilds.text_channels(guild_id, channel_id),
    CONSTRAINT command_id_fk FOREIGN KEY (command_id) REFERENCES commands.commands(name) ON UPDATE CASCADE
);

CREATE TABLE guilds.guild_commands (
    guild_id text NOT NULL,
    command_name text NOT NULL,
    disabled boolean DEFAULT false NOT NULL,
    PRIMARY KEY (guild_id, command_name),
    CONSTRAINT command_fk FOREIGN KEY (command_name) REFERENCES commands.commands(name) ON UPDATE CASCADE,
    CONSTRAINT guild_fk FOREIGN KEY (guild_id) REFERENCES guilds.guilds(guild_id)
);

CREATE TABLE guilds.guild_members (
    guild_id text NOT NULL,
    user_id text NOT NULL,
    first_joined_at timestamp with time zone,
    last_spoke_at timestamp with time zone,
    minute_count integer DEFAULT 0 NOT NULL,
    minutes_milestone integer DEFAULT 0 NOT NULL,
    experience bigint DEFAULT 0 NOT NULL,
    message_count integer DEFAULT 0 NOT NULL,
    word_count integer DEFAULT 0 NOT NULL,
    alive boolean DEFAULT true NOT NULL,
    PRIMARY KEY (guild_id, user_id),
    CONSTRAINT guild_fk FOREIGN KEY (guild_id) REFERENCES guilds.guilds(guild_id),
    CONSTRAINT user_fk FOREIGN KEY (user_id) REFERENCES users.users(user_id)
);

CREATE TABLE guilds.guild_names (
    guild_id text NOT NULL,
    guild_name text NOT NULL,
    changed_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (guild_id, changed_at),
    CONSTRAINT guild_id_fk FOREIGN KEY (guild_id) REFERENCES guilds.guilds(guild_id)
);

CREATE TABLE guilds.guild_role_groups (
    guild_id text NOT NULL,
    role_id text NOT NULL,
    group_name text NOT NULL,
    PRIMARY KEY (guild_id, role_id),
    CONSTRAINT group_name_fk FOREIGN KEY (group_name) REFERENCES commands.user_groups(name),
    CONSTRAINT guild_id_fk FOREIGN KEY (guild_id) REFERENCES guilds.guilds(guild_id)
);

CREATE TABLE guilds.guild_special_roles (
    guild_id text NOT NULL,
    role_id text NOT NULL,
    accessible boolean DEFAULT false NOT NULL,
    PRIMARY KEY (role_id, guild_id),
    CONSTRAINT guild_id_fk FOREIGN KEY (guild_id) REFERENCES guilds.guilds(guild_id)
);

CREATE TABLE guilds.jail_roles (
    guild_id text NOT NULL,
    jail_role_id text NOT NULL,
    set_at timestamp with time zone NOT NULL,
    PRIMARY KEY (guild_id)
);

CREATE TABLE guilds.pro_guilds (
    guild_id text NOT NULL,
    pro_user_id text NOT NULL,
    added_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (guild_id, pro_user_id),
    CONSTRAINT pro_user_id_fk FOREIGN KEY (pro_user_id) REFERENCES users.pro_users(user_id)
);


CREATE SCHEMA attributes;

CREATE TABLE attributes.attributes (
    attribute_id text NOT NULL,
    created_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (attribute_id)
);

CREATE TABLE attributes.birthdays (
    user_id text NOT NULL,
    birthday date NOT NULL,
    last_reward_at timestamp with time zone,
    is_private boolean,
    PRIMARY KEY (user_id),
    CONSTRAINT user_id_fk FOREIGN KEY (user_id) REFERENCES users.users(user_id)
);

CREATE TABLE attributes.integer_attributes (
    attribute_id text NOT NULL,
    user_id text NOT NULL,
    integer_value integer NOT NULL,
    PRIMARY KEY (attribute_id, user_id),
    CONSTRAINT attribute_id_fk FOREIGN KEY (attribute_id) REFERENCES attributes.attributes(attribute_id),
    CONSTRAINT user_id_fk FOREIGN KEY (user_id) REFERENCES users.users(user_id)
);

CREATE TABLE attributes.location_attributes (
    user_id text NOT NULL,
    formatted_address text NOT NULL,
    longitude text NOT NULL,
    latitude text NOT NULL,
    timezone_id text NOT NULL,
    PRIMARY KEY (user_id),
    CONSTRAINT user_id_fk FOREIGN KEY (user_id) REFERENCES users.users(user_id)
);

CREATE TABLE attributes.text_attributes (
    attribute_id text NOT NULL,
    user_id text NOT NULL,
    attribute_value text NOT NULL,
    PRIMARY KEY (attribute_id, user_id),
    CONSTRAINT attribute_id_fk FOREIGN KEY (attribute_id) REFERENCES attributes.attributes(attribute_id),
    CONSTRAINT user_id_fk FOREIGN KEY (user_id) REFERENCES users.users(user_id)
);


CREATE SCHEMA checkers;

CREATE TABLE checkers.instagram_checker (
    instagram_username text NOT NULL,
    guild_id text NOT NULL,
    channel_id text NOT NULL,
    last_post_code text,
    last_taken_at timestamp with time zone DEFAULT '2000-01-01 00:00:00+00'::timestamp with time zone NOT NULL,
    PRIMARY KEY (instagram_username, guild_id, channel_id),
    CONSTRAINT guild_id_fk FOREIGN KEY (guild_id) REFERENCES guilds.guilds(guild_id)
);

CREATE TABLE checkers.reddit_checker (
    guild_id text NOT NULL,
    channel_id text NOT NULL,
    subreddit text NOT NULL,
    last_post_id text,
    last_created timestamp with time zone,
    PRIMARY KEY (guild_id, channel_id, subreddit),
    CONSTRAINT guild_id_fk FOREIGN KEY (guild_id) REFERENCES guilds.guilds(guild_id)
);

CREATE TABLE checkers.tumblr_checker (
    guild_id text NOT NULL,
    channel_id text NOT NULL,
    tumblr_user text NOT NULL,
    last_link text,
    PRIMARY KEY (guild_id, channel_id, tumblr_user),
    CONSTRAINT guild_id_fk FOREIGN KEY (guild_id) REFERENCES guilds.guilds(guild_id)
);

CREATE TABLE checkers.youtube_checker (
    guild_id text NOT NULL,
    channel_id text NOT NULL,
    playlist_id text NOT NULL,
    last_video_id text,
    last_published_at timestamp with time zone,
    PRIMARY KEY (guild_id, channel_id, playlist_id),
    CONSTRAINT guild_id_fk FOREIGN KEY (guild_id) REFERENCES guilds.guilds(guild_id)
);


COMMIT;
