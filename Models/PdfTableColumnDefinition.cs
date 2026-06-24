namespace PdfTemplateApi.Models;

public sealed class PdfTableColumnDefinition
{
    public string Header { get; set; } = "";

    /// <summary>
    /// Token or literal value.
    /// Example: "{{description}}"
    /// </summary>
    public string Value { get; set; } = "";

    /// <summary>
    /// If null, the column uses relative width.
    /// If supplied, the column uses a fixed width.
    /// </summary>
    public float? Width { get; set; }

    /// <summary>
    /// Supported values: left, center, right.
    /// </summary>
    public string? Align { get; set; }
}
