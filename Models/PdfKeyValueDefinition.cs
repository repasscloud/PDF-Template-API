namespace PdfTemplateApi.Models;

public sealed class PdfKeyValueDefinition
{
    public string Label { get; set; } = "";

    public string Value { get; set; } = "";

    public bool? Bold { get; set; }

    public float? FontSize { get; set; }
}
