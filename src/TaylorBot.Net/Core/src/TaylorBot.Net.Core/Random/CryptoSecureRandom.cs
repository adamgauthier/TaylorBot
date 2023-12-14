using System.Security.Cryptography;

namespace TaylorBot.Net.Core.Random;

public interface ICryptoSecureRandom
{
    int GetRandomInt32(int fromInclusive, int toExclusive);
    T GetRandomElement<T>(IReadOnlyList<T> list);
}

public class CryptoSecureRandom : ICryptoSecureRandom
{
    public int GetRandomInt32(int fromInclusive, int toExclusive) => RandomNumberGenerator.GetInt32(fromInclusive, toExclusive);

    public T GetRandomElement<T>(IReadOnlyList<T> list) => list[RandomNumberGenerator.GetInt32(0, list.Count)];
}
