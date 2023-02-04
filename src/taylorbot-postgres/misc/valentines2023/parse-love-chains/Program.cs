using CsvHelper;
using System.Globalization;

using (var reader = new StreamReader(@"..\valentines2023.csv"))
using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
{
    var records = csv.GetRecords<RoleObtained>().ToList();

    var givenTo = records.ToDictionary(r => r.given_to_full_username);

    List<List<RoleObtained>> chains = new();

    foreach (var record in records)
    {
        List<RoleObtained> chain = new() { record };
        Helpers.BuildChain(givenTo, chain, record);
        chain.Reverse();

        chains.Add(chain);
    }

    var longestChain = chains.MaxBy(c => c.Count);
    Console.WriteLine(longestChain.Count);
}

public record RoleObtained(string given_to_full_username, string acquired_from_full_username, string acquired_at);

public static class Helpers
{
    public static void BuildChain(Dictionary<string, RoleObtained> givenTo, List<RoleObtained> chain, RoleObtained end)
    {
        if (end.acquired_from_full_username == end.given_to_full_username)
        {
            return;
        }

        var given = givenTo[end.acquired_from_full_username];
        chain.Add(given);
        BuildChain(givenTo, chain, given);
    }
}
