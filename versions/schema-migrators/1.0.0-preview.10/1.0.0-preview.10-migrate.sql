ALTER TABLE public.users
  SET SCHEMA users;

ALTER TABLE public.usernames
  SET SCHEMA users;

CREATE SCHEMA commands
  AUTHORIZATION postgres;

ALTER TABLE public.commands
  SET SCHEMA commands;

ALTER TABLE public.user_groups
  SET SCHEMA commands;

GRANT USAGE ON SCHEMA commands TO taylorbot;