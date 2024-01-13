-- Revert taylorbot-postgres:0045_birthday_role from pg

BEGIN;

UPDATE commands.messages_of_the_day SET message = 'Just like gambling, you can use `{prefix}heist` to heist your taypoints with friends and win even more!'
WHERE message = 'Create a squad and for a thrilling **Taypoint Bank heist** with </heist play:1183612687935078512>! 💰';

UPDATE commands.messages_of_the_day SET message = 'You can play rock paper scissors with TaylorBot using `{prefix}rps`, you''ll even get a reward if you win!'
WHERE message = 'Use </rps play:1185806478435680387> to play **Rock, Paper, Scissors** with TaylorBot and get rewarded when you win! ✂️';

UPDATE commands.messages_of_the_day SET message = 'You can gamble taypoints to win even more... or lose them! Use `{prefix}gamble` to get started.'
WHERE message = 'Want to **get more taypoints**? Use </risk play:1190786063136993431> to invest your points into sketchy opportunities! 💵';

DROP TABLE plus.birthday_roles_given;

DROP TABLE plus.birthday_roles;

COMMIT;
