ALTER TABLE public.users
  SET SCHEMA users;

ALTER TABLE public.usernames
  SET SCHEMA users;

CREATE SCHEMA commands
  AUTHORIZATION postgres;

ALTER TABLE public.commands
  SET SCHEMA commands;