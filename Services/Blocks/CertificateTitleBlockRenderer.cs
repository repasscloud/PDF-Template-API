using PdfTemplateApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PdfTemplateApi.Services.Blocks;

public sealed class CertificateTitleBlockRenderer : IBlockRenderer
{
    public string Type => "certificateTitle";

    public void Render(
        IContainer container,
        PdfBlockDefinition block,
        PdfRenderContext context)
    {
        var title = BlockRenderHelpers.ResolveValue(block, context, "Certificate");
        var subtitle = BlockRenderHelpers.ResolveString(block.Caption, context);

        container.AlignCenter().Column(column =>
        {
            column.Item()
                .AlignCenter()
                .Text(title)
                .SemiBold()
                .FontSize(block.FontSize ?? 28);

            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                column.Item()
                    .PaddingTop(6)
                    .AlignCenter()
                    .Text(subtitle)
                    .FontSize(12);
            }
        });
    }
}
