using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace TaylorBot.Net.Commands.Discord.Program.Options;

public class HeistOptions
{
    [Required]
    public TimeSpan? TimeSpanBeforeHeistStarts { get; set; }
}

[OptionsValidator]
public partial class HeistOptionsValidator : IValidateOptions<HeistOptions>
{
}
