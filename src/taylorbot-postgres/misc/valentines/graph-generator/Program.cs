using CsvHelper;
using System.Globalization;
using System.Text;

var csvPath = args.Single();
var records = ParseCsv(csvPath);

StringBuilder output = new(
    """
    digraph love_chain {
        rankdir=LR;
        bgcolor="mistyrose";

        node [style=filled, shape=ellipse, fillcolor="lightpink", fontcolor="darkred", fontname="Helvetica"];
        edge [color="red", penwidth=2];


    """);

// Skip first record (self-give from Adam)
foreach (var record in records.Skip(1))
{
    output.AppendLine($"    \"{record.acquired_from_username}\" -> \"{record.username}\";");
}

output.Append("}");

var outputPath = Path.ChangeExtension(csvPath, ".dot");
await File.WriteAllTextAsync(outputPath, output.ToString());
Console.WriteLine(
    $"""
    Wrote output DOT file to {outputPath}
    Convert it to a visual representation using a Graphviz engine or online converters such as https://dreampuf.github.io/GraphvizOnline/
    """);

static List<LoveChainRecord> ParseCsv(string csvPath)
{
    using StreamReader reader = new(csvPath);
    using CsvReader csv = new(reader, CultureInfo.InvariantCulture);
    return [.. csv.GetRecords<LoveChainRecord>()];
}

public record LoveChainRecord(string username, string acquired_from_username, DateTimeOffset acquired_at);
