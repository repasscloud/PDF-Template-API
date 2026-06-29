using PdfTemplateApi.Models;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PdfTemplateApi.Services.Blocks;

public sealed class QrCodeBlockRenderer : IBlockRenderer
{
    public string Type => "qrCode";

    public void Render(
        IContainer container,
        PdfBlockDefinition block,
        PdfRenderContext context)
    {
        var value = BlockRenderHelpers.ResolveValue(block, context);

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("qrCode block requires value, text, or dataPath.");

        using var generator = new QRCodeGenerator();
        using var qrData = generator.CreateQrCode(value, QRCodeGenerator.ECCLevel.Q);

        var qrCode = new PngByteQRCode(qrData);
        var bytes = qrCode.GetGraphic(40);

        var size = block.Size ?? block.Width ?? 100;
        var caption = BlockRenderHelpers.ResolveString(block.Caption, context);

        var aligned = BlockRenderHelpers.ApplyAlignment(container, block.Align);

        aligned.Width(size).Column(column =>
        {
            column.Item()
                .Width(size)
                .Height(size)
                .Image(bytes)
                .FitArea();

            if (!string.IsNullOrWhiteSpace(caption))
            {
                column.Item()
                    .PaddingTop(4)
                    .AlignCenter()
                    .Text(caption)
                    .FontSize(8);
            }
        });
    }
}
