using PdfTemplateApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PdfTemplateApi.Services.Blocks;

public sealed class SignatureBlockRenderer : IBlockRenderer
{
    public string Type => "signature";

    public void Render(
        IContainer container,
        PdfBlockDefinition block,
        PdfRenderContext context)
    {
        var label = BlockRenderHelpers.ResolveString(block.Label ?? "Signature", context);
        var name = BlockRenderHelpers.ResolveString(block.Value, context);
        var caption = BlockRenderHelpers.ResolveString(block.Caption, context);

        var width = block.Width ?? 240;
        var height = block.Height ?? 48;

        var aligned = BlockRenderHelpers.ApplyAlignment(container, block.Align);

        aligned.Width(width).Column(column =>
        {
            column.Item().Height(height);

            column.Item()
                .LineHorizontal(1);

            column.Item()
                .PaddingTop(4)
                .Text(label)
                .FontSize(block.FontSize ?? 9);

            if (!string.IsNullOrWhiteSpace(name))
            {
                column.Item()
                    .Text(name)
                    .FontSize(9)
                    .SemiBold();
            }

            if (!string.IsNullOrWhiteSpace(caption))
            {
                column.Item()
                    .Text(caption)
                    .FontSize(8);
            }
        });
    }
}
