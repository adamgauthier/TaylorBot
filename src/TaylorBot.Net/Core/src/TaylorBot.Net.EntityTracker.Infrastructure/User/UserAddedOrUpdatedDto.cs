namespace TaylorBot.Net.EntityTracker.Infrastructure.User
{
    public class UserAddedOrUpdatedDto
    {
        public bool was_inserted { get; set; }
        public bool username_changed { get; set; }
        public string previous_username { get; set; }
    }
}
