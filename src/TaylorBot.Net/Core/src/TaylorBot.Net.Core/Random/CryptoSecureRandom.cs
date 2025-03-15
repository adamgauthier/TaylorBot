using System.Security.Cryptography;

namespace TaylorBot.Net.Core.Random;

public interface IRandom
{
    int GetInt32(int fromInclusive, int toInclusive);
    int GetInt32Exclusive(int fromInclusive, int toExclusive);
    T GetRandomElement<T>(IReadOnlyList<T> list);
}

public interface ICryptoSecureRandom : IRandom
{
}

public class CryptoSecureRandom : ICryptoSecureRandom
{
    public int GetInt32(int fromInclusive, int toInclusive) =>
        GetInt32Exclusive(fromInclusive, toInclusive + 1);

    public int GetInt32Exclusive(int fromInclusive, int toExclusive) =>
        RandomNumberGenerator.GetInt32(fromInclusive, toExclusive);

    public T GetRandomElement<T>(IReadOnlyList<T> list) =>
        list[GetInt32Exclusive(0, list.Count)];
}

public interface IPseudoRandom : IRandom
{
}

public class PseudoRandom : IPseudoRandom
{
    private readonly System.Random random = new();
    private readonly Lock lockObject = new();

    public int GetInt32(int fromInclusive, int toInclusive)
    {
        return GetInt32Exclusive(fromInclusive, toInclusive + 1);
    }

    public int GetInt32Exclusive(int fromInclusive, int toExclusive)
    {
        lock (lockObject)
        {
            return random.Next(fromInclusive, toExclusive);
        }
    }

    public T GetRandomElement<T>(IReadOnlyList<T> list)
    {
        return list[GetInt32Exclusive(0, list.Count)];
    }
}
