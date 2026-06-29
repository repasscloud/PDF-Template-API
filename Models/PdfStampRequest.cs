namespace PdfTemplateApi.Models;

public sealed class PdfStampRequest
{
    /// <summary>
    /// Base64-encoded PDF to stamp.
    /// </summary>
    public string PdfBase64 { get; set; } = "";

    /// <summary>
    /// Large status text displayed inside the stamp box.
    /// Example: "APPROVED", "REJECTED", "PAID"
    /// </summary>
    public string Status { get; set; } = "APPROVED";

    /// <summary>
    /// Optional secondary line — typically "Approved by Name".
    /// </summary>
    public string? Value { get; set; }

    /// <summary>
    /// Optional caption line — typically a date.
    /// </summary>
    public string? Caption { get; set; }

    /// <summary>
    /// Hex colour for the stamp border and text.
    /// Green: "#166534" (default). Red: "#991B1B".
    /// </summary>
    public string Color { get; set; } = "#166534";

    /// <summary>
    /// Where to place the stamp on each page.
    /// Supported: topLeft, topRight (default), bottomLeft, bottomRight, center.
    /// </summary>
    public string Position { get; set; } = "topRight";

    /// <summary>
    /// Stamp box width in points.
    /// </summary>
    public float Width { get; set; } = 180;

    /// <summary>
    /// Optional filename for the Content-Disposition header.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Apply the stamp to all pages. Defaults to true.
    /// When false, only the first page is stamped.
    /// </summary>
    public bool AllPages { get; set; } = true;
}
