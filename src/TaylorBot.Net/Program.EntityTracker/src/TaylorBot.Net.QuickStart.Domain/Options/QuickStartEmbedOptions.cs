using System.Collections.Generic;

namespace TaylorBot.Net.QuickStart.Domain.Options
{
    public class QuickStartEmbedOptions
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public IEnumerable<QuickStartEmbedField> Fields { get; set; } = null!;
        public string Color { get; set; } = null!;
    }
}
