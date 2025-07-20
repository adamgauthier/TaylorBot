-- Deploy taylorbot-postgres:20250720_prefix_commands_disable to pg

BEGIN;

INSERT INTO commands.commands (name, disabled_message, added_at, aliases, module_name)
VALUES ('all-prefix', '', DEFAULT, '{}', '');

COMMIT;
