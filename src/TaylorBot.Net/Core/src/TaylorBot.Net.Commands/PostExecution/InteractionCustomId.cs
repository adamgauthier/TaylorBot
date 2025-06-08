namespace TaylorBot.Net.Commands.PostExecution;

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

    public IList<CustomIdDataEntry> DataEntries => [.. Data
        .Split(DataSeparator)
        .Select(s => s.Split(DataRowSeparator))
        .Select(s => new CustomIdDataEntry(s[0], s[1]))];

    public Dictionary<string, string> ParsedData => DataEntries
        .ToDictionary(e => e.Key, e => e.Value);

    public static InteractionCustomId Create(string name, IList<CustomIdDataEntry>? data = null) =>
        new($"{0}{Separator}{name}{Separator}{(data != null ? string.Join(DataSeparator, data.Select(kvp => $"{kvp.Key}{DataRowSeparator}{kvp.Value}")) : "")}");

    public static InteractionCustomId Create(CustomIdNames name, IList<CustomIdDataEntry>? data = null) =>
        Create(name.ToText(), data);
}

public enum CustomIdNames
{
    // 0-100 are reserved for generic ids
    Unknown = 0,
    GenericPromptCancel = 1,
    GenericMessageDelete = 2,
    PageMessagePrevious = 3,
    PageMessageNext = 4,
    PageMessageCancel = 5,

    ModMailUserMessageReply = 101,
    ModMailUserMessageReplyModal = 102,
    ModMailReplyConfirm = 103,
    RemindManageClear = 104,
    RemindManageClearAll = 105,
    BirthdayRoleCreate = 106,
    BirthdayRoleRemove = 107,
    BirthdaySetConfirm = 108,
    DailyRebuyConfirm = 109,
    FavoriteBaeSetConfirm = 110,
    FavoriteObsessionSetConfirm = 111,
    FavoriteSongsSetConfirm = 112,
    KickConfirm = 113,
    ModLogSetConfirm = 114,
    ModMailConfigConfirm = 115,
    ModMailMessageModsConfirm = 116,
    MonitorDeletedSetConfirm = 117,
    MonitorEditedSetConfirm = 118,
    MonitorMembersSetConfirm = 119,
    TaypointsGiftConfirm = 120,
    SignatureConfirm = 121,
    ModMailMessageModsModal = 122,
    TaypointsSuccessionClaim = 123,
    TaypointsSuccessionClaimSkip = 124,
    TaypointsSuccessionChangeSuccessor = 125,
    TaypointsSuccessionClearSuccessor = 126,
    ValentineGiveawayEnter = 127,
    ModMailConfigSetChannel = 128,
    ModMailConfigStop = 129,
    HelpCategory = 130,
    ModLogStop = 131,
    MonitorEditedStop = 132,
    MonitorDeletedStop = 133,
    MonitorMembersStop = 134,
}

public static class CustomIdExtensions
{
    public static string ToText(this CustomIdNames customIdName) => $"{(int)customIdName}";
}
