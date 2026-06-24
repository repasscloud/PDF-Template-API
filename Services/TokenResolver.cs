using System.Text.Json;
using System.Text.RegularExpressions;

namespace PdfTemplateApi.Services;

public sealed partial class TokenResolver
{
    private static readonly Regex TokenRegex = CreateTokenRegex();

    public string Apply(
        string input,
        JsonElement rootData,
        JsonElement? localData = null)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return TokenRegex.Replace(input, match =>
        {
            var path = match.Groups["path"].Value.Trim();

            if (localData is not null &&
                TryResolveJsonPath(localData.Value, path, out var localValue))
            {
                return JsonToString(localValue);
            }

            if (TryResolveJsonPath(rootData, path, out var rootValue))
                return JsonToString(rootValue);

            // Leave unresolved tokens visible.
            // This makes bad templates/data obvious when testing.
            return match.Value;
        });
    }

    public bool TryResolveJsonPath(
        JsonElement root,
        string path,
        out JsonElement value)
    {
        value = default;

        if (root.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
            return false;

        path = NormalizePath(path);

        if (string.IsNullOrWhiteSpace(path))
        {
            value = root;
            return true;
        }

        var current = root;

        foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
        {
            if (current.ValueKind == JsonValueKind.Object)
            {
                if (!TryGetPropertyIgnoreCase(current, segment, out current))
                    return false;

                continue;
            }

            if (current.ValueKind == JsonValueKind.Array &&
                int.TryParse(segment, out var index))
            {
                var array = current.EnumerateArray().ToArray();

                if (index < 0 || index >= array.Length)
                    return false;

                current = array[index];
                continue;
            }

            return false;
        }

        value = current;
        return true;
    }

    private static bool TryGetPropertyIgnoreCase(
        JsonElement element,
        string propertyName,
        out JsonElement value)
    {
        foreach (var property in element.EnumerateObject())
        {
            if (string.Equals(
                property.Name,
                propertyName,
                StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private static string NormalizePath(string path)
    {
        path = path.Trim();

        if (path.StartsWith("$.", StringComparison.Ordinal))
            return path[2..];

        if (path.StartsWith("$", StringComparison.Ordinal))
            return path[1..];

        return path;
    }

    private static string JsonToString(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => string.Empty,
            JsonValueKind.Undefined => string.Empty,
            _ => element.GetRawText()
        };
    }

    [GeneratedRegex(@"\{\{\s*(?<path>[a-zA-Z0-9_\.\-\$]+)\s*\}\}")]
    private static partial Regex CreateTokenRegex();
}
