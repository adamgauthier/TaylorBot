-- Deploy taylorbot-postgres:0045_birthday_role to pg

BEGIN;

CREATE TABLE plus.birthday_roles (
    guild_id text NOT NULL,
    role_id text NOT NULL,
    set_at timestamp with time zone DEFAULT now() NOT NULL,
    PRIMARY KEY (guild_id)
);

CREATE TABLE plus.birthday_roles_given (
    guild_id text NOT NULL,
    user_id text NOT NULL,
    role_id text NOT NULL,
    set_at timestamp with time zone NOT NULL,
    remove_at timestamp with time zone NOT NULL,
    removed_at timestamp with time zone DEFAULT NULL,
    PRIMARY KEY (guild_id, user_id)
);

UPDATE commands.messages_of_the_day SET message = 'Want to **get more taypoints**? Use </risk play:1190786063136993431> to invest your points into sketchy opportunities! 💵'
WHERE message = 'You can gamble taypoints to win even more... or lose them! Use `{prefix}gamble` to get started.';

UPDATE commands.messages_of_the_day SET message = 'Use </rps play:1185806478435680387> to play **Rock, Paper, Scissors** with TaylorBot and get rewarded when you win! ✂️'
WHERE message = 'You can play rock paper scissors with TaylorBot using `{prefix}rps`, you''ll even get a reward if you win!';

UPDATE commands.messages_of_the_day SET message = 'Create a squad and for a thrilling **Taypoint Bank heist** with </heist play:1183612687935078512>! 💰'
WHERE message = 'Just like gambling, you can use `{prefix}heist` to heist your taypoints with friends and win even more!';

COMMIT;
