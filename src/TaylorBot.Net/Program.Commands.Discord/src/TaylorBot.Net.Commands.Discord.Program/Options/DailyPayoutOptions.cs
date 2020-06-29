namespace TaylorBot.Net.Commands.Discord.Program.Options
{
    public class DailyPayoutOptions
    {
        public uint DailyPayoutAmount { get; set; }
        public uint DaysForBonus { get; set; }
        public uint BaseBonusAmount { get; set; }
        public uint IncreasingBonusModifier { get; set; }
    }
}
