namespace TaylorBot.Net.Core.Snowflake
{
    public class SnowflakeId
    {
        public ulong Id { get; }

        public SnowflakeId(ulong id)
        {
            Id = id;
        }

        public SnowflakeId(string id)
        {
            Id = ulong.Parse(id);
        }

        public override string ToString()
        {
            return Id.ToString();
        }
    }
}
