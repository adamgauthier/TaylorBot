using Discord.Commands;
using TaylorBot.Net.Commands.DiscordNet;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Removed.Commands;

[Name("Removed 👋")]
public class RemovedModule(PrefixedCommandRunner prefixedCommandRunner) : TaylorBotModule
{
    [Command("instagram")]
    [Alias("insta", "setinstagram", "set instagram", "setinsta", "set insta", "clearinstagram", "clear instagram", "clearinsta", "clear insta")]
    public async Task<RuntimeResult> InstagramAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(IsRemoved: true, RemovedMessage: "Please use Discord's profile connections feature instead! 😊"));

    [Command("snapchat")]
    [Alias("snap", "setsnapchat", "set snapchat", "setsnap", "set snap", "clearsnapchat", "clear snapchat", "clearsnap", "clear snap", "listsnapchat", "list snapchat", "listsnap", "list snap")]
    public async Task<RuntimeResult> SnapchatAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(IsRemoved: true));

    [Command("tumblr")]
    [Alias("settumblr", "set tumblr", "cleartumblr", "clear tumblr", "listtumblr", "list tumblr")]
    public async Task<RuntimeResult> TumblrAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(IsRemoved: true));

    [Command("wikipedia")]
    [Alias("wiki")]
    public async Task<RuntimeResult> WikipediaAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(IsRemoved: true));

    [Command("subreddit")]
    [Alias("sub")]
    public async Task<RuntimeResult> SubredditAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(IsRemoved: true));

    [Command("poll")]
    [Alias("reactpoll", "rpoll")]
    public async Task<RuntimeResult> ReactPollAsync([Remainder] string? _ = null) => await prefixedCommandRunner.RunAsync(
        Context,
        new(IsRemoved: true, RemovedMessage: "Polls are now natively supported by Discord! 🙏"));
}
