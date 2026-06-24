using PdfTemplateApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PdfTemplateApi.Services.Blocks;

public sealed class BarcodeBlockRenderer : IBlockRenderer
{
    private readonly BarcodeImageGenerator _barcodes;

    public BarcodeBlockRenderer(BarcodeImageGenerator barcodes)
    {
        _barcodes = barcodes;
    }

    public string Type => "barcode";

    public void Render(
        IContainer container,
        PdfBlockDefinition block,
        PdfRenderContext context)
    {
        var value = BlockRenderHelpers.ResolveValue(block, context);

        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("barcode block requires value, text, or dataPath.");

        var width = (int)(block.Width ?? 280);
        var height = (int)(block.Height ?? 80);
        var format = block.Label ?? "CODE_128";
        var caption = BlockRenderHelpers.ResolveString(block.Caption, context);

        var bytes = _barcodes.Generate(value, format, width, height);

        var aligned = BlockRenderHelpers.ApplyAlignment(container, block.Align);

        aligned.Width(width).Column(column =>
        {
            column.Item()
                .Width(width)
                .Height(height)
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
