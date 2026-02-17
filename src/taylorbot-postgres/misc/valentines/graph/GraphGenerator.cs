#:package CsvHelper@33.0.1

using CsvHelper;
using System.Globalization;
using System.Text;

var csvPath = args.Single();
var records = ParseCsv(csvPath);

// Build hierarchy
Dictionary<string, List<string>> children = [];
Dictionary<string, string> parentMap = [];

foreach (var record in records)
{
    if (record.username == record.acquired_from_username)
        continue;

    if (!children.ContainsKey(record.acquired_from_username))
        children[record.acquired_from_username] = [];
    children[record.acquired_from_username].Add(record.username);
    parentMap[record.username] = record.acquired_from_username;
}

// Sort adam's direct children alphabetically for consistent left-to-right ordering
if (children.TryGetValue("adam", out var adamChildren))
    adamChildren.Sort(StringComparer.OrdinalIgnoreCase);

// Color palette per chain
string[] colors =
[
    "#E8B4B8", "#F4A6A3", "#E57373", "#EF9A9A",
    "#A5D6A7", "#81C784", "#66BB6A", "#4CAF50",
    "#FFF59D", "#FFEE58", "#FFEB3B", "#FDD835",
    "#FFB74D", "#FFA726", "#FF9800", "#FB8C00",
    "#90CAF9", "#64B5F6", "#42A5F5", "#2196F3",
    "#CE93D8", "#BA68C8", "#AB47BC", "#9C27B0",
    "#80CBC4", "#4DB6AC", "#26A69A", "#009688",
];

HashSet<string> botUsernames = ["LosingHimWasBlue", "BenjiBot", "BiancaBot", "FearlessBot", "TaylorBot"];

const string root = "adam";

// Auto-detect dead chains
Dictionary<string, bool> deadChains = [];
if (children.TryGetValue(root, out var rootChildren))
{
    foreach (var child in rootChildren)
    {
        var leaf = FindLeaf(child);
        if (botUsernames.Contains(leaf))
            deadChains[child] = true;
    }
}

string FindLeaf(string node)
{
    var current = node;
    while (children.TryGetValue(current, out var c) && c.Count > 0)
        current = c[0];
    return current;
}

string GetChainColor(string node)
{
    var current = node;
    while (parentMap.TryGetValue(current, out var parent) && parent != root)
        current = parent;

    if (current == root)
        return "#888888";

    if (children.TryGetValue(root, out var rc))
    {
        var idx = rc.IndexOf(current);
        return idx >= 0 ? colors[idx % colors.Length] : "#888888";
    }
    return "#888888";
}

static string CleanId(string name) => "user_" + name.Replace('.', '_').Replace('-', '_');

// Build DOT output
StringBuilder output = new(
    """
    digraph love_chain {
        rankdir=TB;
        bgcolor="#1a1a1a";
        splines=ortho;
        nodesep=0.8;
        ranksep=0.8;

        node [style="filled,rounded", shape=box, fontcolor="#1a1a1a", fontname="Helvetica-Bold", fontsize=16, width=2.2, height=0.7, margin=0.15];
        edge [color="#666666", penwidth=2];

        ordering=out;

    """);

HashSet<string> visited = [];

void AddNodesAndEdges(string node)
{
    if (!visited.Add(node))
        return;

    var color = GetChainColor(node);
    var nodeId = CleanId(node);
    output.AppendLine($"    \"{nodeId}\" [label=\"{node}\", fillcolor=\"{color}\"];");

    if (children.TryGetValue(node, out var nodeChildren))
    {
        foreach (var child in nodeChildren)
        {
            AddNodesAndEdges(child);
            output.AppendLine($"    \"{nodeId}\" -> \"{CleanId(child)}\";");
        }
    }
}

AddNodesAndEdges(root);

// Add tombstones for dead chains
if (children.TryGetValue(root, out var rootKids))
{
    foreach (var child in rootKids)
    {
        if (!deadChains.ContainsKey(child))
            continue;

        var last = FindLeaf(child);
        var lastId = CleanId(last);
        var deadId = $"dead_{CleanId(child)}";
        output.AppendLine($"    \"{deadId}\" [label=\"🪦 Rest In Peace\", fillcolor=\"#222222\", fontcolor=\"#ffb6b6\", width=1.6, height=0.6];");
        output.AppendLine($"    \"{lastId}\" -> \"{deadId}\" [penwidth=3, style=dotted];");
    }
}

output.Append('}');

var outputPath = Path.ChangeExtension(csvPath, ".dot");
await File.WriteAllTextAsync(outputPath, output.ToString());
Console.WriteLine(
    $"""
    Wrote output DOT file to {outputPath} ({visited.Count} people, {rootChildren?.Count ?? 0} chains)
    Convert it to a visual representation using a Graphviz engine or online converters such as https://dreampuf.github.io/GraphvizOnline/
    """);

static List<LoveChainRecord> ParseCsv(string csvPath)
{
    using StreamReader reader = new(csvPath);
    using CsvReader csv = new(reader, CultureInfo.InvariantCulture);
    return [.. csv.GetRecords<LoveChainRecord>()];
}

public record LoveChainRecord(string username, string acquired_from_username, DateTimeOffset acquired_at);
