UPDATE public.guild_members
SET first_joined_at = 9223372036854775807
WHERE first_joined_at = 0;