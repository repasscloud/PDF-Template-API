using PdfTemplateApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PdfTemplateApi.Services.Blocks;

public sealed class TwoColumnBlockRenderer : IBlockRenderer
{
    public string Type => "twoColumn";

    public void Render(
        IContainer container,
        PdfBlockDefinition block,
        PdfRenderContext context)
    {
        var leftBlocks = block.LeftBlocks ?? [];
        var rightBlocks = block.RightBlocks ?? [];

        var gap = block.Gap ?? 20;
        var leftWidth = block.LeftWidth ?? 1;
        var rightWidth = block.RightWidth ?? 1;

        container.Row(row =>
        {
            row.RelativeItem(leftWidth).Column(column =>
            {
                foreach (var child in leftBlocks)
                    context.RenderBlock(column.Item(), child);
            });

            row.ConstantItem(gap);

            row.RelativeItem(rightWidth).Column(column =>
            {
                foreach (var child in rightBlocks)
                    context.RenderBlock(column.Item(), child);
            });
        });
    }
}
