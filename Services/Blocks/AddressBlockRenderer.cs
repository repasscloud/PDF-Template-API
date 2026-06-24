using PdfTemplateApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PdfTemplateApi.Services.Blocks;

public sealed class AddressBlockRenderer : IBlockRenderer
{
    public string Type => "addressBlock";

    public void Render(
        IContainer container,
        PdfBlockDefinition block,
        PdfRenderContext context)
    {
        var label = BlockRenderHelpers.ResolveString(block.Label, context);
        var lines = block.Lines ?? [];

        var content = block.Padding is > 0
            ? container.Padding(block.Padding.Value)
            : container;

        content.Column(column =>
        {
            if (!string.IsNullOrWhiteSpace(label))
            {
                column.Item()
                    .Text(label)
                    .SemiBold()
                    .FontSize(block.FontSize ?? 10);
            }

            foreach (var line in lines)
            {
                var resolved = BlockRenderHelpers.ResolveString(line, context);

                if (string.IsNullOrWhiteSpace(resolved))
                    continue;

                column.Item()
                    .Text(resolved)
                    .FontSize(block.FontSize ?? 9);
            }
        });
    }
}
