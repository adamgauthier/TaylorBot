ALTER TABLE public.guild_commands
DROP CONSTRAINT command_fk,
ADD CONSTRAINT command_fk
   FOREIGN KEY (command_name)
   REFERENCES public.commands(name)
   ON UPDATE CASCADE;

UPDATE public.commands
SET name = 'addaccessiblerole'
WHERE name = 'setaccessiblerole';

UPDATE public.commands
SET name = 'disablecommand'
WHERE name = 'disableglobal';

UPDATE public.commands
SET name = 'enablecommand'
WHERE name = 'enableglobal';

ALTER TABLE public.guild_members ALTER COLUMN first_joined_at DROP DEFAULT;

DELETE FROM public.commands WHERE name = 'prefix' OR name = 'eval';