namespace PdfTemplateApi.Models;

public sealed class PdfTemplateDefinition
{
    public string? Title { get; set; }

    /// <summary>
    /// Supported values in this sample: A4, Letter.
    /// </summary>
    public string PageSize { get; set; } = "A4";

    /// <summary>
    /// Supported values: Portrait, Landscape.
    /// </summary>
    public string Orientation { get; set; } = "Portrait";

    public float Margin { get; set; } = 40;

    public float DefaultFontSize { get; set; } = 10;

    public float BlockSpacing { get; set; } = 8;

    public bool ShowPageNumbers { get; set; } = true;

    public List<PdfBlockDefinition> Blocks { get; set; } = [];
}
