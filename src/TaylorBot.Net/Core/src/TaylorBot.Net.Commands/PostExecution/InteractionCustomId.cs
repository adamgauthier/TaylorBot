﻿namespace TaylorBot.Net.Commands.PostExecution;

public record CustomIdDataEntry(string Key, string Value);

public record InteractionCustomId(string RawId)
{
    public const char Separator = '|';
    public const char DataSeparator = '&';
    public const char DataRowSeparator = '=';

    public string[] Split() => RawId.Split(Separator);

    public bool IsValid => Split().Length == 3;

    public string Version => Split()[0];

    public string Name => Split()[1];

    public CustomIdNames ParsedName => Enum.TryParse(Name, out CustomIdNames name) ? name : CustomIdNames.Unknown;

    public string Data => Split()[2];

    public List<CustomIdDataEntry> DataEntries => Data
        .Split(DataSeparator)
        .Select(s => s.Split(DataRowSeparator))
        .Select(s => new CustomIdDataEntry(s[0], s[1]))
        .ToList();

    public Dictionary<string, string> ParsedData => DataEntries
        .ToDictionary(e => e.Key, e => e.Value);

    public static InteractionCustomId Create(string name, IReadOnlyList<CustomIdDataEntry>? data = null) =>
        new($"{0}{Separator}{name}{Separator}{(data != null ? string.Join(DataSeparator, data.Select(kvp => $"{kvp.Key}{DataRowSeparator}{kvp.Value}")) : "")}");

    public static InteractionCustomId Create(CustomIdNames name, IReadOnlyList<CustomIdDataEntry>? data = null) =>
        Create(name.ToText(), data);
}

public enum CustomIdNames
{
    // 0-100 are reserved for generic ids
    Unknown = 0,
    GenericPromptCancel = 1,

    ModMailUserMessageReply = 101,
    ModMailUserMessageReplyModal = 102,
    ModMailReplyConfirm = 103,
}

public static class CustomIdExtensions
{
    public static string ToText(this CustomIdNames customIdName) => $"{(int)customIdName}";
}
