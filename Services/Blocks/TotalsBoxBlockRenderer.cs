using PdfTemplateApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PdfTemplateApi.Services.Blocks;

public sealed class TotalsBoxBlockRenderer : IBlockRenderer
{
    public string Type => "totalsBox";

    public void Render(
        IContainer container,
        PdfBlockDefinition block,
        PdfRenderContext context)
    {
        var rows = block.KeyValues ?? [];

        if (rows.Count == 0)
            throw new InvalidOperationException("totalsBox requires keyValues.");

        var width = block.Width ?? 240;
        var padding = block.Padding ?? 8;

        var aligned = BlockRenderHelpers.ApplyAlignment(container, block.Align ?? "right");

        aligned.Width(width)
            .Border(1)
            .Padding(padding)
            .Column(column =>
            {
                foreach (var rowItem in rows)
                {
                    var label = BlockRenderHelpers.ResolveString(rowItem.Label, context);
                    var value = BlockRenderHelpers.ResolveString(rowItem.Value, context);
                    var fontSize = rowItem.FontSize ?? block.FontSize ?? 10;
                    var bold = rowItem.Bold == true;

                    column.Item().Row(row =>
                    {
                        row.RelativeItem()
                            .Text(text =>
                            {
                                var span = text.Span(label).FontSize(fontSize);

                                if (bold)
                                    span.SemiBold();
                            });

                        row.ConstantItem(90)
                            .AlignRight()
                            .Text(text =>
                            {
                                var span = text.Span(value).FontSize(fontSize);

                                if (bold)
                                    span.SemiBold();
                            });
                    });
                }
            });
    }
}
