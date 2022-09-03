-- Revert taylorbot-postgres:0036_slash_command_mentions from pg

BEGIN;

UPDATE commands.messages_of_the_day SET message = 'Ask TaylorBot to remind you about something using **/remind add**!'
WHERE message = 'Ask TaylorBot to remind you about something using </remind add:861754955728027678>!';

UPDATE commands.messages_of_the_day SET message = 'For a limited time, use **/daily claim** to get **more taypoints every day**!'
WHERE message = 'For a limited time, use </daily claim:870731803739168859> to get **more taypoints every day**!';

UPDATE commands.messages_of_the_day SET message = 'If you have a **TaylorBot Plus** server, you can monitor deleted and edited messages. Use **/monitor deleted set** and **/monitor edited set** to get started!'
WHERE message = 'If you have a **TaylorBot Plus** server, you can monitor deleted and edited messages. Use </monitor deleted set:887146682146488390> and </monitor edited set:887146682146488390> to get started!';

UPDATE commands.messages_of_the_day SET message = 'Lost a longtime daily streak? Re-buy it with **/daily rebuy**!'
WHERE message = 'Lost a longtime daily streak? Re-buy it with </daily rebuy:870731803739168859>!';

UPDATE commands.messages_of_the_day SET message = 'Set up your Last.fm account with **/lastfm set**, use **/lastfm current** to show your now playing, use **/lastfm albums**, **/lastfm artists** and **/lastfm tracks** to show off what you listen to!'
WHERE message = 'Set up your Last.fm account with </lastfm set:922354806574678086>, use </lastfm current:922354806574678086> to show your now playing, use </lastfm albums:922354806574678086>, </lastfm artists:922354806574678086> and </lastfm tracks:922354806574678086> to show off what you listen to!';

UPDATE commands.messages_of_the_day SET message = 'Try the **new Slash Command experience** by typing `/avatar`! No more typos in the command format or forgetting the server prefix!'
WHERE message = 'Try the **new Slash Command experience** by using </avatar:832103922709692436>! No more typos in the command format or forgetting the server prefix!';

UPDATE commands.messages_of_the_day SET message = 'Use **/choose** to have TaylorBot make a decision for you!'
WHERE message = 'Use </choose:843563366751404063> to have TaylorBot make a decision for you!';

UPDATE commands.messages_of_the_day SET message = 'Use **/mod log set** to set up a channel to record moderation command usage!'
WHERE message = 'Use </mod log set:838266590294048778> to set up a channel to record moderation command usage!';

UPDATE commands.messages_of_the_day SET message = 'You can send and receive messages through your moderation team. As a moderator, use **/mod mail message-user** and as a user, use **/mod mail message-mods**!'
WHERE message = 'You can send and receive messages through your moderation team. As a moderator, use </mod mail message-user:838266590294048778> and as a user, use </mod mail message-mods:838266590294048778>!';

COMMIT;
