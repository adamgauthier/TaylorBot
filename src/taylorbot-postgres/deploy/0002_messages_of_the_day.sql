-- Deploy taylorbot-postgres:0002_messages_of_the_day to pg

BEGIN;

INSERT INTO commands.messages_of_the_day (message) VALUES
('You can gamble taypoints to win even more... or lose them! Use `{prefix}gamble` to get started.');

INSERT INTO commands.messages_of_the_day (message) VALUES
('Just like gambling, you can use `{prefix}heist` to heist your taypoints with friends and win even more!');

INSERT INTO commands.messages_of_the_day (message) VALUES
('Use `{prefix}setbirthday` to set your birthday and get points when the time comes!');

INSERT INTO commands.messages_of_the_day (message) VALUES
('Use `{prefix}time` to see what time it is for any user that has set their location!');

INSERT INTO commands.messages_of_the_day (message) VALUES
('TaylorBot has a variety of features, use `{prefix}help` to get started with interesting ones!');

INSERT INTO commands.messages_of_the_day (message) VALUES
('TaylorBot remembers the date when you first joined a server. Use `{prefix}joined` to see yours!');

INSERT INTO commands.messages_of_the_day (message) VALUES
('Use `{prefix}choose` to have TaylorBot make a decision for you!');

INSERT INTO commands.messages_of_the_day (message) VALUES
('Use `{prefix}status` to show off your Discord status, including your currently playing song on Spotify!');

INSERT INTO commands.messages_of_the_day (message) VALUES
('Start a poll and see what others think using `{prefix}poll`!');

INSERT INTO commands.messages_of_the_day (message) VALUES
('You can play rock paper scissors with TaylorBot using `{prefix}rps`, you''ll even get a reward if you win!');

INSERT INTO commands.messages_of_the_day (message) VALUES
('Are star signs really the one true science? Use `{prefix}horoscope` to get started.');

INSERT INTO commands.messages_of_the_day (message) VALUES
('Feeling generous? Use `{prefix}gift` to send some of your points to a friend!');

INSERT INTO commands.messages_of_the_day (message) VALUES
('Scared of what happens to your points if you lose your account? Use `{prefix}taypointwill add`!');

INSERT INTO commands.messages_of_the_day (message) VALUES
('TaylorBot is funded by the community, thanks to our Patreon supporters. Learn more with `{prefix}support`.');

INSERT INTO commands.messages_of_the_day (message) VALUES
('Set up your Last.fm account with `{prefix}fm set`, use `{prefix}fm` to show your now playing, use `{prefix}fm albums`, `{prefix}fm artists` and `{prefix}fm tracks` to show off what you listen to the most!');

COMMIT;
