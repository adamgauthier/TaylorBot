using System.Collections.Generic;

namespace TaylorBot.Net.QuickStart.Domain.Options
{
    public class QuickStartEmbedOptions
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public IEnumerable<QuickStartEmbedField> Fields { get; set; }
        public string Color { get; set; }
    }
}
