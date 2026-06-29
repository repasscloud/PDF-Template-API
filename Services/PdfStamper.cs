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
        const float statusFontSize = 15f;
        const float valueFontSize = 8.5f;
        const float captionFontSize = 8f;
        const float innerPadding = 8f;
        const float borderWidth = 2f;

        var stampWidth = request.Width;

        // Calculate stamp height based on how many lines are present.
        var stampHeight = innerPadding * 2 + statusFontSize + 4;
        if (!string.IsNullOrWhiteSpace(request.Value)) stampHeight += valueFontSize + 4;
        if (!string.IsNullOrWhiteSpace(request.Caption)) stampHeight += captionFontSize + 3;

        var (x, y) = StampPosition(pageSize, request.Position, stampWidth, stampHeight);

        // Use NewContentStreamAfter so stamp renders on top of existing content.
        var canvas = new PdfCanvas(page.NewContentStreamAfter(), page.GetResources(), pdfDoc);

        // Border rectangle.
        canvas.SetStrokeColor(new DeviceRgb(r, g, b))
              .SetLineWidth(borderWidth)
              .Rectangle(x, y, stampWidth, stampHeight)
              .Stroke();

        canvas.SetFillColor(new DeviceRgb(r, g, b));

        // Track vertical position from top of stamp (PDF y-origin is bottom-left).
        var textY = y + stampHeight - innerPadding - statusFontSize;

        // Status line (bold, larger).
        DrawCenteredText(canvas, request.Status, boldFont, statusFontSize, x, textY, stampWidth);
        textY -= statusFontSize + 4;

        if (!string.IsNullOrWhiteSpace(request.Value))
        {
            DrawCenteredText(canvas, request.Value!, regularFont, valueFontSize, x, textY, stampWidth);
            textY -= valueFontSize + 3;
        }

        if (!string.IsNullOrWhiteSpace(request.Caption))
        {
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
        string position,
        float stampWidth,
        float stampHeight)
    {
        const float margin = 28f;

        return position.ToLowerInvariant() switch
        {
            "topleft" => (margin, page.GetHeight() - margin - stampHeight),
            "bottomleft" => (margin, margin),
            "bottomright" => (page.GetWidth() - margin - stampWidth, margin),
            "center" => (
                (page.GetWidth() - stampWidth) / 2f,
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
