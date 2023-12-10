using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace TaylorBot.Net.Commands.Discord.Program.Options;

public class HeistOptions
{
    [Required]
    [MinLength(1)]
    public string Banks { get; set; } = null!;
    [Required]
    public TimeSpan TimeSpanBeforeHeistStarts { get; set; }
}

[OptionsValidator]
public partial class HeistOptionsValidator : IValidateOptions<HeistOptions>
{
}
