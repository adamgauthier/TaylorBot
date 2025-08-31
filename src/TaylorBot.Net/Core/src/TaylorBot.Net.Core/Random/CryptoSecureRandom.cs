using System.Security.Cryptography;

namespace TaylorBot.Net.Core.Random;

public interface IRandom
{
    int GetInt32Exclusive(int fromInclusive, int toExclusive);

    int GetInt32(int fromInclusive, int toInclusive) =>
        GetInt32Exclusive(fromInclusive, toInclusive + 1);

    T GetRandomElement<T>(IReadOnlyList<T> list) =>
        list[GetInt32Exclusive(0, list.Count)];
}

public interface ICryptoSecureRandom : IRandom
{
}

public class CryptoSecureRandom : ICryptoSecureRandom
{
    public int GetInt32Exclusive(int fromInclusive, int toExclusive) =>
        RandomNumberGenerator.GetInt32(fromInclusive, toExclusive);
}

public interface IPseudoRandom : IRandom
{
}

public class PseudoRandom : IPseudoRandom
{
    public int GetInt32Exclusive(int fromInclusive, int toExclusive) =>
        System.Random.Shared.Next(fromInclusive, toExclusive);
}
