﻿namespace TaylorBot.Net.MessageLogging.Domain.Options
{
    public class MessageDeletedLoggingOptions
    {
        public string MessageDeletedEmbedColorHex { get; set; } = null!;
        public bool UseRedisCache { get; set; }
    }
}
