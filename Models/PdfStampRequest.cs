namespace PdfTemplateApi.Models;

/// <summary>
/// Request body for POST /pdf/stamp.
///
/// COORDINATE SYSTEM
/// -----------------
/// PDF origin (0, 0) is at the BOTTOM-LEFT of each page. X increases rightward,
/// Y increases upward.
///
/// Common page sizes in points (1 pt = 1/72 inch ≈ 0.353 mm, 1 cm ≈ 28.35 pt):
///   A4 Portrait  : 595 × 842 pt  (21.0 × 29.7 cm)
///   A4 Landscape : 842 × 595 pt  (29.7 × 21.0 cm)
///   Letter       : 612 × 792 pt  (21.6 × 27.9 cm)
///
/// The stamp box is positioned by its BOTTOM-LEFT corner (X, Y).
/// So to place a 125 × 84 pt stamp in the top-right corner of an A4 portrait page
/// with 28 pt of margin:
///   X = 595 - 28 - 125 = 442
///   Y = 842 - 28 -  84 = 730
///
/// POSITIONING
/// -----------
/// Supply either X + Y for exact placement, or Position for a named corner.
/// X/Y take priority — if both are present, Position is ignored.
///
/// Named positions (Position field):
///   topRight    (default)  — 28 pt from top-right corner
///   topLeft                — 28 pt from top-left corner
///   bottomRight            — 28 pt from bottom-right corner
///   bottomLeft             — 28 pt from bottom-left corner
///   center                 — centred on the page
/// </summary>
public sealed class PdfStampRequest
{
    /// <summary>Base64-encoded PDF bytes to stamp.</summary>
    public string PdfBase64 { get; set; } = "";

    /// <summary>
    /// Large bold status text. Example: "APPROVED", "REJECTED", "PAID", "VERIFIED".
    /// </summary>
    public string Status { get; set; } = "APPROVED";

    /// <summary>Optional second line — typically "Approved by Name".</summary>
    public string? Value { get; set; }

    /// <summary>Optional third line — typically a date.</summary>
    public string? Caption { get; set; }

    /// <summary>
    /// Hex colour for the border and all text. Defaults to dark green #166534.
    /// Common values: green "#166534", red "#991B1B", orange "#C2500A", navy "#1A3A5C".
    /// </summary>
    public string Color { get; set; } = "#166534";

    // -------------------------------------------------------------------------
    // Positioning — supply X + Y, or supply Position. X/Y take priority.
    // -------------------------------------------------------------------------

    /// <summary>
    /// Exact X coordinate of the stamp's left edge in PDF points (origin bottom-left).
    /// Must be supplied together with Y. Takes priority over Position when both are set.
    ///
    /// Example (A4 portrait, stamp 125 pt wide, 28 pt from right edge):
    ///   X = 595 - 28 - 125 = 442
    /// </summary>
    public float? X { get; set; }

    /// <summary>
    /// Exact Y coordinate of the stamp's bottom edge in PDF points (origin bottom-left).
    /// Must be supplied together with X. Takes priority over Position when both are set.
    ///
    /// Example (A4 portrait, stamp 84 pt tall, 28 pt from top edge):
    ///   Y = 842 - 28 - 84 = 730
    /// </summary>
    public float? Y { get; set; }

    /// <summary>
    /// Named corner position used when X and Y are not supplied.
    /// Accepted values: topRight (default), topLeft, bottomRight, bottomLeft, center.
    /// The stamp is placed 28 pt inside the chosen corner.
    /// </summary>
    public string Position { get; set; } = "topRight";

    // -------------------------------------------------------------------------

    /// <summary>Stamp box width in points. Default 180 pt (≈ 6.4 cm).</summary>
    public float Width { get; set; } = 180;

    /// <summary>Optional Content-Disposition filename returned with the response.</summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Stamp every page when true (default). Set false to stamp only the first page.
    /// </summary>
    public bool AllPages { get; set; } = true;
}
