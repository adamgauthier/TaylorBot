using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace TaylorBot.Net.Commands.Discord.Program.Options;

public class SerpApiOptions
{
    [Required]
    public string ApiKey { get; set; } = null!;
}

[OptionsValidator]
public partial class SerpApiOptionsValidator : IValidateOptions<SerpApiOptions>
{
}
