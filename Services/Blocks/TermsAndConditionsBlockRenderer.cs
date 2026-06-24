using PdfTemplateApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PdfTemplateApi.Services.Blocks;

public sealed class TermsAndConditionsBlockRenderer : IBlockRenderer
{
    public string Type => "termsAndConditions";

    public void Render(
        IContainer container,
        PdfBlockDefinition block,
        PdfRenderContext context)
    {
        var title = BlockRenderHelpers.ResolveString(block.Label ?? "Terms and Conditions", context);
        var paragraph = BlockRenderHelpers.ResolveString(block.Text, context);
        var lines = block.Lines ?? [];

        container.Column(column =>
        {
            column.Item()
                .Text(title)
                .SemiBold()
                .FontSize(block.FontSize ?? 11);

            if (!string.IsNullOrWhiteSpace(paragraph))
            {
                column.Item()
                    .PaddingTop(4)
                    .Text(paragraph)
                    .FontSize(8);
            }

            for (var i = 0; i < lines.Count; i++)
            {
                var line = BlockRenderHelpers.ResolveString(lines[i], context);

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                column.Item()
                    .PaddingTop(3)
                    .Text($"{i + 1}. {line}")
                    .FontSize(8);
            }
        });
    }
}
