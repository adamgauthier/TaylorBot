namespace TaylorBot.Net.Core.Snowflake
{
    public record SnowflakeId(ulong Id)
    {
        public SnowflakeId(string id) : this(ulong.Parse(id)) { }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}
