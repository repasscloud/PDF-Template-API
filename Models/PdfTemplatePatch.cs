namespace PdfTemplateApi.Models;

public sealed class PdfTemplatePatch
{
    public string? Title { get; set; }

    public string? PageSize { get; set; }

    public string? Orientation { get; set; }

    public float? Margin { get; set; }

    public float? DefaultFontSize { get; set; }

    public float? BlockSpacing { get; set; }

    public bool? ShowPageNumbers { get; set; }

    /// <summary>
    /// If supplied, this replaces the template's existing blocks.
    /// </summary>
    public List<PdfBlockDefinition>? Blocks { get; set; }
}
