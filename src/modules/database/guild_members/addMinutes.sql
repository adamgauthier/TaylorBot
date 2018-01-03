BEGIN;

--Add Minutes
UPDATE public.guild_members
SET minutes_count = minutes_count + ${minutes_to_add}
WHERE last_spoke_at > ${min_spoke_at};

--Update Minutes Milestones
UPDATE public.guild_members
SET
minutes_milestone = (minutes_count-(minutes_count % ${minutes_for_reward})),
taypoints_count = taypoints_count + ${reward_count}
WHERE minutes_count >= minutes_milestone + ${minutes_for_reward};

COMMIT;