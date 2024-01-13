using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace TaylorBot.Net.BirthdayReward.Domain.Options;

public class BirthdayRoleOptions
{
    [Required]
    public TimeSpan? TimeSpanBetweenAdding { get; set; }

    [Required]
    public TimeSpan? TimeSpanBetweenRemoving { get; set; }
}

[OptionsValidator]
public partial class BirthdayRoleOptionsValidator : IValidateOptions<BirthdayRoleOptions>
{
}
