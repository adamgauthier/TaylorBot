CREATE OR REPLACE FUNCTION pg_temp.reward_users(user_ids text[], reward_points integer)
RETURNS TABLE(_user_id text) AS $$
BEGIN
    RETURN QUERY
    UPDATE users.users
    SET taypoint_count = taypoint_count + reward_points
    WHERE users.user_id = ANY(user_ids)
    RETURNING user_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION pg_temp.reward_users_for_event(user_ids text[], reward_points integer)
RETURNS TABLE(_reward_message text) AS $$
BEGIN
    RETURN QUERY
    SELECT CONCAT('You just got **', reward_points::text, ' taypoints** for **EVENT NAME**! Thank you <@', string_agg(_user_id, '> <@'), '>')
    FROM pg_temp.reward_users(user_ids, reward_points);
END;
$$ LANGUAGE plpgsql;

SELECT * FROM pg_temp.reward_users_for_event('{1, 2, 3}', 1);
