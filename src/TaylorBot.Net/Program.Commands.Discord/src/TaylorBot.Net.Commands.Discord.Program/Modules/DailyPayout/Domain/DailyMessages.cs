namespace TaylorBot.Net.Commands.Discord.Program.Modules.DailyPayout.Domain;

public static class DailyMessages
{
    public static readonly IReadOnlyList<MessageOfTheDay> Default =
    [
        new MessageOfTheDay(
            Id: new("bdbf99e2-e05e-47c8-a496-f680230dc816"),
            Message: "TaylorBot has a variety of features, use `/help` to get started with interesting ones!"
        ),
        new MessageOfTheDay(
            Id: new("759c3edd-11f6-4453-b534-ee26be889654"),
            Message: "Scared of what happens to your points if you lose your account? Use `/taypoints succession`!"
        ),
        new MessageOfTheDay(
            Id: new("75322cb4-6907-40d1-b965-932a13e4ec5e"),
            Message: "Ask TaylorBot to remind you about something using `/remind add`!"
        ),
        new MessageOfTheDay(
            Id: new("5eca526d-6e3d-4e07-8f06-d1192cf0d5ef"),
            Message: "For a limited time, use `/daily claim` to get **more taypoints every day**!"
        ),
        new MessageOfTheDay(
            Id: new("5fb1e913-60b2-45f0-981a-a30ee2af76fa"),
            Message: "If you have a **TaylorBot Plus** server, you can monitor deleted and edited messages. Use `/monitor deleted set` and `/monitor edited set` to get started!"
        ),
        new MessageOfTheDay(
            Id: new("bd179f86-151b-415d-a5ab-e13ede5adac5"),
            Message: "Lost a longtime daily streak? Re-buy it with `/daily rebuy`!"
        ),
        new MessageOfTheDay(
            Id: new("49f7e90f-9e94-45b3-9e06-579af0263587"),
            Message: "Set up your Last.fm account with `/lastfm set`, use `/lastfm current` to show your now playing, use `/lastfm albums`, `/lastfm artists` and `/lastfm tracks` to show off what you listen to!"
        ),
        new MessageOfTheDay(
            Id: new("89353e7c-d9c1-4e32-bacf-190dd3aa31bd"),
            Message: "Try the **new Slash Command experience** by using `/avatar`! No more typos in the command format or forgetting the server prefix!"
        ),
        new MessageOfTheDay(
            Id: new("a01285c3-0ba6-4e33-ac08-6c893ecbaae8"),
            Message: "Use `/choose` to have TaylorBot make a decision for you!"
        ),
        new MessageOfTheDay(
            Id: new("c8a267eb-c0cf-470c-a911-85c7cb97fa45"),
            Message: "Use `/mod log set` to set up a channel to record moderation command usage!"
        ),
        new MessageOfTheDay(
            Id: new("ffa60263-485a-4fcc-81b2-6c42f907807e"),
            Message: "Are star signs really the one true science? Use `/birthday horoscope` to get started."
        ),
        new MessageOfTheDay(
            Id: new("f8bac92d-d59d-4a81-9251-bef0950c964b"),
            Message: "Use `/birthday set` to set your birthday and get points when the time comes!"
        ),
        new MessageOfTheDay(
            Id: new("1824346e-f97a-44c6-8d23-77767f39c8c2"),
            Message: "You can send and receive messages through your moderation team. As a moderator, use `/modmail message-user` and as a user, use `/modmail message-mods`!"
        ),
        new MessageOfTheDay(
            Id: new("4a58f37d-2214-4057-abe7-aa623b5c7ddf"),
            Message: "Use `/location time` to see what time it is for your friend in a different part of the world!"
        ),
        new MessageOfTheDay(
            Id: new("92699d06-c2a2-4434-a875-148f592a0dcb"),
            Message: "Feeling generous? Use `/taypoints gift` to send some of your taypoints to a friend!"
        ),
        new MessageOfTheDay(
            Id: new("9e81a91e-b712-4594-9ea7-cc5a60490543"),
            Message: "Use `/location weather` to see the current weather where you or your friend are!"
        ),
        new MessageOfTheDay(
            Id: new("7e8f4ac1-53ff-49e4-b9b2-af3e56b87f1c"),
            Message: "TaylorBot keeps track of how active you are in each server. Use `/server minutes`, `/server messages` and `/server leaderboard` in you favorite server!"
        ),
        new MessageOfTheDay(
            Id: new("a2623aea-a2d2-4017-b81b-4064e3939041"),
            Message: "TaylorBot remembers the date when you first joined a server. Use `/server joined` to see yours!"
        ),
        new MessageOfTheDay(
            Id: new("fd5c00a2-99f1-4248-8a5e-54684a5c034f"),
            Message: "Want to **get more taypoints**? Use `/risk play` to invest your points into sketchy opportunities! 💵"
        ),
        new MessageOfTheDay(
            Id: new("abae79b6-7a60-4e01-aee2-562c8dc859c5"),
            Message: "Use `/rps play` to play **Rock, Paper, Scissors** with TaylorBot and get rewarded when you win! ✂️"
        ),
        new MessageOfTheDay(
            Id: new("85d35493-5382-48c8-8d0f-309cc3f6c41c"),
            Message: "Don't recognize someone? Use `/usernames show` to check if they changed their username! Use `/usernames visibility` to make your history private! 🔍"
        ),
        new MessageOfTheDay(
            Id: new("04ca3a40-f9c3-40f4-8165-23839e262f46"),
            Message: "Subscribe to [TaylorBot Plus](https://www.patreon.com/taylorbot) to get **exclusive features and taypoints**, learn more with `/plus show` 💎"
        ),
        new MessageOfTheDay(
            Id: new("ac7524b0-15a6-4472-a4bc-e747208f52c4"),
            Message: "On **TaylorBot's profile**, tap **Add App** -> **Add to Server** to add TaylorBot to your own server! 🤖"
        ),
        new MessageOfTheDay(
            Id: new("bd505eb4-a75e-49a8-bb35-942bc75aa272"),
            Message:
            """
            Did you know you can **use the bot everywhere on Discord**? 🎉
            - Add **TaylorBot** to your account, allowing you to use even in servers where **TaylorBot** is not added 🌐
            - On **TaylorBot's profile**, tap **Add App** -> **Add to My Apps** ✅
            - You can also go to **DMs** with your friends and type **/** to use **TaylorBot** commands ✨
            """
        ),
        new MessageOfTheDay(
            Id: new("3386758b-94e6-48f7-a9f2-b66e99f44a57"),
            Message: "Create a squad for a thrilling **Taypoint Bank heist** with `/heist play`! 💰"
        )
    ];
}
