namespace TaylorBot.Net.Core.Snowflake;

public record SnowflakeId(ulong Id)
{
    public SnowflakeId(string id) : this(ulong.Parse(id)) { }

    public override string ToString() => $"{Id}";

    public static implicit operator string(SnowflakeId id) => $"{id}";
    public static implicit operator ulong(SnowflakeId id) => id.Id;
    public static implicit operator SnowflakeId(string id) => new(id);
    public static implicit operator SnowflakeId(ulong id) => new(id);
}
