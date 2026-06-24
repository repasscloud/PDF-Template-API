using PdfTemplateApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PdfTemplateApi.Services.Blocks;

public sealed class ApprovalStampBlockRenderer : IBlockRenderer
{
    public string Type => "approvalStamp";

    public void Render(
        IContainer container,
        PdfBlockDefinition block,
        PdfRenderContext context)
    {
        var status = BlockRenderHelpers.ResolveString(block.Status ?? "APPROVED", context);
        var by = BlockRenderHelpers.ResolveString(block.Value, context);
        var caption = BlockRenderHelpers.ResolveString(block.Caption, context);

        var width = block.Width ?? 180;
        var padding = block.Padding ?? 8;
        var color = block.Color ?? "#166534";

        var aligned = BlockRenderHelpers.ApplyAlignment(container, block.Align ?? "right");

        aligned.Width(width)
            .Border(2)
            .BorderColor(color)
            .Padding(padding)
            .Column(column =>
            {
                column.Item()
                    .AlignCenter()
                    .Text(status)
                    .SemiBold()
                    .FontSize(block.FontSize ?? 16)
                    .FontColor(color);

                if (!string.IsNullOrWhiteSpace(by))
                {
                    column.Item()
                        .PaddingTop(4)
                        .AlignCenter()
                        .Text(by)
                        .FontSize(9)
                        .FontColor(color);
                }

                if (!string.IsNullOrWhiteSpace(caption))
                {
                    column.Item()
                        .AlignCenter()
                        .Text(caption)
                        .FontSize(8)
                        .FontColor(color);
                }
            });
    }
}
