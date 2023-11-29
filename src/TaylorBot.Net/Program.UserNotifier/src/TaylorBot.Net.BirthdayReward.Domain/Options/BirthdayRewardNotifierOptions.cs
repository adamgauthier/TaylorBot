namespace TaylorBot.Net.BirthdayReward.Domain.Options
{
    public class BirthdayRewardNotifierOptions
    {
        public uint RewardAmount { get; set; }
        public TimeSpan TimeSpanBetweenRewards { get; set; }
        public TimeSpan TimeSpanBetweenMessages { get; set; }
    }
}
