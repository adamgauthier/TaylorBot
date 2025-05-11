using Microsoft.Extensions.Caching.Memory;

namespace TaylorBot.Net.Commands.PageMessages;

public class PageOptionsInMemoryRepository(IMemoryCache cache)
{
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(15);

    private static string GetKey(Guid optionsId) => $"pageoptions-{optionsId:N}";

    public void Register(PageOptions options) =>
        cache.Set(GetKey(options.Id), options, CacheExpiration);

    public void Remove(Guid optionsId) =>
        cache.Remove(GetKey(optionsId));

    public PageOptions? Get(Guid optionsId) =>
        cache.TryGetValue<PageOptions>(GetKey(optionsId), out var options) ? options : null;
}
