using System.Text.Json;

namespace PdfTemplateApi.Models;

public sealed class PdfBuildRequest
{
    /// <summary>
    /// Name of a JSON template file in /Templates without the .json extension.
    /// Example: "invoice" loads /Templates/invoice.json.
    /// </summary>
    public string? TemplateName { get; set; }

    /// <summary>
    /// Optional full inline template.
    /// If provided, it replaces the named template.
    /// </summary>
    public PdfTemplateDefinition? Template { get; set; }

    /// <summary>
    /// Optional partial override applied after the template is loaded.
    /// Useful for changing title, margin, page size, or replacing blocks.
    /// </summary>
    public PdfTemplatePatch? TemplatePatch { get; set; }

    /// <summary>
    /// Data used by {{tokens}} inside blocks.
    /// </summary>
    public JsonElement Data { get; set; }

    public PdfOutputOptions? Output { get; set; }
}
