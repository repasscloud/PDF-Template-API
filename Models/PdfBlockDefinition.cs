namespace PdfTemplateApi.Models;

public sealed class PdfBlockDefinition
{
    /// <summary>
    /// Supported block types:
    /// heading, text, spacer, line, table, pageBreak,
    /// signature, qrCode, image, twoColumn, addressBlock,
    /// totalsBox, watermark, termsAndConditions, barcode,
    /// certificateTitle, approvalStamp
    /// </summary>
    public string Type { get; set; } = "";

    public string? Text { get; set; }

    public string? Value { get; set; }

    public string? Label { get; set; }

    public string? Caption { get; set; }

    public int? Level { get; set; }

    public float? FontSize { get; set; }

    public bool? Bold { get; set; }

    public bool? Italic { get; set; }

    public string? Align { get; set; }

    public string? Color { get; set; }

    public string? BackgroundColor { get; set; }

    public float? Width { get; set; }

    public float? Height { get; set; }

    public float? Size { get; set; }

    public float? Padding { get; set; }

    public float? Gap { get; set; }

    public float? LeftWidth { get; set; }

    public float? RightWidth { get; set; }

    public string? DataPath { get; set; }

    public string? Source { get; set; }

    public string? Base64 { get; set; }

    public string? Fit { get; set; }

    public string? Status { get; set; }

    public bool? ShowHeader { get; set; }

    public List<string>? Lines { get; set; }

    public List<PdfKeyValueDefinition>? KeyValues { get; set; }

    public List<PdfTableColumnDefinition>? Columns { get; set; }

    public List<List<string>>? Rows { get; set; }

    public List<PdfBlockDefinition>? LeftBlocks { get; set; }

    public List<PdfBlockDefinition>? RightBlocks { get; set; }
}
