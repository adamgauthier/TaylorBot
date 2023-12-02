using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace TaylorBot.Net.Commands.Discord.Program.Options;

public class ImgurOptions
{
    [Required]
    [MinLength(1)]
    public string ClientId { get; set; } = null!;
}

[OptionsValidator]
public partial class ImgurOptionsValidator : IValidateOptions<ImgurOptions>
{
}
