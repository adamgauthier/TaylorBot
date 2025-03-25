﻿using Discord;
using Humanizer;
using System.Collections.Concurrent;
using System.Globalization;
using TaylorBot.Net.Commands.Parsers;
using TaylorBot.Net.Commands.PostExecution;
using TaylorBot.Net.Commands.Preconditions;
using TaylorBot.Net.Core.Embed;
using TaylorBot.Net.Core.Number;
using TaylorBot.Net.Core.Time;
using static TaylorBot.Net.Commands.MessageResult;

namespace TaylorBot.Net.Commands.Discord.Program.Modules.Poll.Commands;

public class PollSlashCommand(InGuildPrecondition.Factory inGuild) : ISlashCommand<NoOptions>
{
    public static readonly Color PollColor = new(84, 160, 255);

    public static string CommandName => "poll";

    public ISlashCommandInfo Info => new ModalCommandInfo(CommandName);

    public ValueTask<Command> GetCommandAsync(RunContext context, NoOptions _)
    {
        return new(new Command(
            new(Info.Name),
            () =>
            {
                return new(new CreateModalResult(
                    Id: "poll-create",
                    Title: "Create a Poll",
                    TextInputs:
                    [
                        new TextInput(Id: "title", TextInputStyle.Short, Label: "Poll Title", MaxLength: 250),
                        new TextInput(Id: "option1", TextInputStyle.Short, Label: "Option 1", MaxLength: 150),
                        new TextInput(Id: "option2", TextInputStyle.Short, Label: "Option 2", MaxLength : 150),
                        new TextInput(Id: "option3", TextInputStyle.Short, Label: "Option 3 (if applicable)", Required: false, MaxLength: 150),
                        new TextInput(Id: "option4", TextInputStyle.Short, Label: "Option 4 (if applicable)", Required: false, MaxLength: 150)
                    ],
                    SubmitAction: submit =>
                    {
                        Poll poll = new(submit);
                        return new(poll.CreateResult());
                    },
                    IsPrivateResponse: false
                ));
            },
            Preconditions: [inGuild.Create()]
        ));
    }

    private sealed class Poll
    {
        private static readonly List<string> Icons =
        [
            "1️⃣",
            "2️⃣",
            "3️⃣",
            "4️⃣"
        ];
        private static readonly List<string> WinningIcons =
        [
            "🥇",
            "🥈",
            "🥉",
            "🎀"
        ];
        private sealed record Option(short Id, string Name, string Icon);

        private readonly TimeSpan _duration;
        private readonly DateTimeOffset _startedAt;
        private readonly DateTimeOffset _endsAt;
        private readonly string _title;
        private readonly List<Option> _options = [];
        private readonly ConcurrentDictionary<string, short> _voters = new();

        private DateTimeOffset? _endedAt;

        public Poll(ModalSubmit submit)
        {
            _duration = TimeSpan.FromMinutes(10);
            _startedAt = DateTimeOffset.UtcNow;
            _endsAt = _startedAt + _duration;
            _title = submit.TextInputs.Single(t => t.CustomId == "title").Value;

            foreach (var option in submit.TextInputs.Where(t => t.CustomId.StartsWith("option", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrWhiteSpace(t.Value)))
            {
                var id = short.Parse(option.CustomId[6..]);
                _options.Add(new(id, option.Value, Icons[id - 1]));
            }
        }

        public MessageResult CreateResult()
        {
            List<ButtonResult> buttons = [];

            foreach (var option in _options)
            {
                Button button = new($"vote-{option.Id}", ButtonStyle.Primary, Label: "", option.Icon);

                buttons.Add(new(button, OnClickVote(option), AllowNonAuthor: true));
            }

            buttons.Add(new(new("end-poll", ButtonStyle.Danger, Label: "End", "🛑"), OnClickEnd));

            return new MessageResult(
                new(CreateEmbed()),
                new(buttons, new TemporaryButtonSettings(_duration, OnEnded: OnEnded)));
        }

        private Func<string, ValueTask<IButtonClickResult>> OnClickVote(Option option)
        {
            return userId =>
            {
                if (_endedAt != null)
                {
                    return new(new IgnoreClick());
                }

                if (_voters.TryGetValue(userId, out var votedId))
                {
                    if (votedId == option.Id)
                    {
                        if (_voters.TryRemove(userId, out var removed))
                        {
                            return new(new UpdateMessageContent(
                                new(CreateEmbed()),
                                Response: new(new(new(EmbedFactory.CreateSuccess(
                                    $"<@{userId}> Your vote for {option.Icon} has been removed! 🗑️🗳️"))),
                                    IsPrivate: true)));
                        }
                        else
                        {
                            return new(new IgnoreClick());
                        }
                    }
                    else
                    {
                        if (_voters.TryUpdate(userId, option.Id, votedId))
                        {
                            return new(new UpdateMessageContent(
                                new(CreateEmbed()),
                                Response: new(new(new(EmbedFactory.CreateSuccess(
                                    $"<@{userId}> Your vote for {_options.Single(o => o.Id == votedId).Icon} has been changed to {option.Icon}! ➡️🗳️"))),
                                    IsPrivate: true)));
                        }
                        else
                        {
                            return new(new IgnoreClick());
                        }
                    }
                }
                else
                {
                    var wasAdded = _voters.TryAdd(userId, option.Id);
                    if (wasAdded)
                    {
                        return new(new UpdateMessageContent(
                            new(CreateEmbed()),
                            Response: new(new(new(EmbedFactory.CreateSuccess(
                                $"<@{userId}> Your vote for {option.Icon} has been recorded! ✅🗳️"))),
                                IsPrivate: true)));
                    }
                    else
                    {
                        return new(new IgnoreClick());
                    }
                }
            };
        }

        private ValueTask<IButtonClickResult> OnClickEnd(string userId)
        {
            _endedAt = DateTimeOffset.UtcNow;
            return new(new UpdateMessage(new(new(CreateEmbed()))));
        }

        private ValueTask<MessageResult> OnEnded()
        {
            _endedAt = DateTimeOffset.UtcNow;
            return new(new MessageResult(new(CreateEmbed())));
        }

        public Embed CreateEmbed()
        {
            var voters = _voters.ToList();
            Dictionary<short, HashSet<string>> votesByOption = [];

            foreach (var vote in voters)
            {
                if (votesByOption.TryGetValue(vote.Value, out var votersForOption))
                {
                    votersForOption.Add(vote.Key);
                }
                else
                {
                    votesByOption[vote.Value] = [vote.Key];
                }
            }

            var options = _options.Select(o =>
            {
                var count = votesByOption.TryGetValue(o.Id, out var votersForOption) ? votersForOption.Count : 0;
                return (
                    Count: count,
                    Icon: o.Icon,
                    Displayed: $"{o.Name} - {FormatPercent(count, voters.Count)} ({"vote".ToQuantity(count, TaylorBotFormats.Readable)})");
            });

            string displayedOptions = string.Join('\n', GetDisplayedOptions(options));

            return new EmbedBuilder()
                .WithColor(PollColor)
                .WithDescription(
                    $"""
                    ## {_title}
                    {displayedOptions}

                    {(_endedAt.HasValue
                        ? $"Poll lasted {(_endedAt.Value - _startedAt).Humanize()}, closed {_endedAt.Value.FormatRelative()} ✅"
                        : $"Vote using the buttons below, the poll closes {_endsAt.FormatRelative()} ⏱️")}
                    """)
                .Build();
        }

        private static string FormatPercent(int count, int totalVoters)
        {
            var percent = totalVoters != 0 ? (decimal)count / totalVoters : 0;
            return percent.ToString("0%", CultureInfo.InvariantCulture);
        }

        private IEnumerable<string> GetDisplayedOptions(IEnumerable<(int Count, string Icon, string Displayed)> options)
        {
            if (_endedAt != null)
            {
                var sorted = options.OrderByDescending(o => o.Count).ToList();
                var rank = 0;
                var displayed = sorted.Select((o, i) =>
                {
                    if (i != 0 && o.Count < sorted[i - 1].Count)
                    {
                        rank++;
                    }
                    return $"### {WinningIcons[rank]} {o.Displayed}";
                });

                return displayed;
            }
            else
            {
                return options.Select(o => $"### {o.Icon} {o.Displayed}");
            }
        }
    }
}
