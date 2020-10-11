﻿using System.Collections.Generic;
using System.Linq;

namespace TaylorBot.Net.Core.Embed
{
    public static class StringEnumerableEmbedExtensions
    {
        public static string CreateEmbedDescriptionWithMaxAmountOfLines(this IEnumerable<string> lines)
        {
            const int EmbedDescriptionMaxLength = 2048;

            return string.Join('\n',
                lines.TakeWhile((line, index) =>
                    string.Join('\n', lines.Take(index + 1)).Length <= EmbedDescriptionMaxLength
                )
            );
        }
    }
}