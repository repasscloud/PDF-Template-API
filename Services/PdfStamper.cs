using PdfTemplateApi.Models;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Geom;

namespace PdfTemplateApi.Services;

public sealed class PdfStamper
{
    public byte[] ApplyStamp(PdfStampRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PdfBase64))
            throw new InvalidOperationException("pdfBase64 is required.");

        byte[] pdfBytes;

        try
        {
            pdfBytes = Convert.FromBase64String(request.PdfBase64);
        }
        catch (FormatException)
        {
            throw new InvalidOperationException("pdfBase64 is not valid base64.");
        }

        var (r, g, b) = ParseHexColor(request.Color);

        using var inputStream = new MemoryStream(pdfBytes);
        using var reader = new PdfReader(inputStream);
        using var outputStream = new MemoryStream();
        using var writer = new PdfWriter(outputStream);
        using var pdfDoc = new PdfDocument(reader, writer);

        var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
        var regularFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

        var pageCount = pdfDoc.GetNumberOfPages();
        var lastPage = request.AllPages ? pageCount : 1;

        for (var i = 1; i <= lastPage; i++)
        {
            var page = pdfDoc.GetPage(i);
            var pageSize = page.GetPageSize();
            StampPage(pdfDoc, page, pageSize, request, r, g, b, boldFont, regularFont);
        }

        pdfDoc.Close();
        return outputStream.ToArray();
    }

    private static void StampPage(
        PdfDocument pdfDoc,
        PdfPage page,
        Rectangle pageSize,
        PdfStampRequest request,
        float r, float g, float b,
        PdfFont boldFont,
        PdfFont regularFont)
    {
        // Font sizes and spacing — tweak these to resize the stamp.
        const float statusFontSize = 22f;
        const float valueFontSize  = 11f;
        const float captionFontSize = 10f;
        const float innerPadding = 13f;   // top + bottom padding inside the box
        const float lineGap = 5f;          // gap between each text line
        const float borderWidth = 2.5f;

        var stampWidth = request.Width;

        // Height: top padding + status + optional (gap+value) + optional (gap+caption) + bottom padding.
        var stampHeight = innerPadding * 2 + statusFontSize;
        if (!string.IsNullOrWhiteSpace(request.Value))   stampHeight += lineGap + valueFontSize;
        if (!string.IsNullOrWhiteSpace(request.Caption)) stampHeight += lineGap + captionFontSize;

        var (x, y) = StampPosition(pageSize, request, stampWidth, stampHeight);

        var canvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdfDoc);

        // QuestPDF (via SkiaSharp) writes [0.25 0 0 -0.25 0 pageHeight] cm at the top of
        // every page content stream WITHOUT a surrounding q/Q, so the flipped+scaled CTM
        // bleeds into every subsequent content stream on the same page.
        // Cancel it with the inverse matrix [4 0 0 -4 0 4*pageHeight] so we work in
        // standard PDF coordinates (origin bottom-left, Y increases upward, 1 unit = 1 pt).
        canvas.ConcatMatrix(4, 0, 0, -4, 0, 4.0 * pageSize.GetHeight());

        canvas.SetStrokeColor(new DeviceRgb(r, g, b))
              .SetLineWidth(borderWidth)
              .Rectangle(x, y, stampWidth, stampHeight)
              .Stroke();

        canvas.SetFillColor(new DeviceRgb(r, g, b));

        // Position text working down from the top of the content area.
        // In PDF coords Y increases upward, so "down" means subtracting.
        var textY = y + stampHeight - innerPadding - statusFontSize;

        DrawCenteredText(canvas, request.Status, boldFont, statusFontSize, x, textY, stampWidth);

        if (!string.IsNullOrWhiteSpace(request.Value))
        {
            textY -= lineGap + valueFontSize;
            DrawCenteredText(canvas, request.Value!, regularFont, valueFontSize, x, textY, stampWidth);
        }

        if (!string.IsNullOrWhiteSpace(request.Caption))
        {
            textY -= lineGap + captionFontSize;
            DrawCenteredText(canvas, request.Caption!, regularFont, captionFontSize, x, textY, stampWidth);
        }

        canvas.Release();
    }

    private static void DrawCenteredText(
        PdfCanvas canvas,
        string text,
        PdfFont font,
        float fontSize,
        float boxX,
        float baselineY,
        float boxWidth)
    {
        var textWidth = font.GetWidth(text, fontSize);
        var textX = boxX + (boxWidth - textWidth) / 2f;

        canvas.BeginText()
              .SetFontAndSize(font, fontSize)
              .MoveText(textX, baselineY)
              .ShowText(text)
              .EndText();
    }

    private static (float x, float y) StampPosition(
        Rectangle page,
        PdfStampRequest request,
        float stampWidth,
        float stampHeight)
    {
        // Explicit coordinates take priority over the named position.
        if (request.X.HasValue && request.Y.HasValue)
            return (request.X.Value, request.Y.Value);

        const float margin = 28f;

        return request.Position.ToLowerInvariant() switch
        {
            "topleft"     => (margin, page.GetHeight() - margin - stampHeight),
            "bottomleft"  => (margin, margin),
            "bottomright" => (page.GetWidth() - margin - stampWidth, margin),
            "center"      => (
                (page.GetWidth()  - stampWidth)  / 2f,
                (page.GetHeight() - stampHeight) / 2f),
            _ => (page.GetWidth() - margin - stampWidth, page.GetHeight() - margin - stampHeight)
        };
    }

    private static (float r, float g, float b) ParseHexColor(string hex)
    {
        hex = hex.TrimStart('#');

        if (hex.Length == 6)
        {
            return (
                Convert.ToInt32(hex[0..2], 16) / 255f,
                Convert.ToInt32(hex[2..4], 16) / 255f,
                Convert.ToInt32(hex[4..6], 16) / 255f);
        }

        // Default: dark green #166534
        return (0.086f, 0.396f, 0.204f);
    }
}
