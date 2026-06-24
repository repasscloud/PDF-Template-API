using System.Text.Json;
using PdfTemplateApi.Models;
using PdfTemplateApi.Services.Blocks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PdfTemplateApi.Services;

public sealed class PdfRenderer
{
    private readonly IReadOnlyDictionary<string, IBlockRenderer> _blockRenderers;
    private readonly TokenResolver _tokens;

    public PdfRenderer(
        IEnumerable<IBlockRenderer> blockRenderers,
        TokenResolver tokens)
    {
        _blockRenderers = blockRenderers.ToDictionary(
            x => x.Type,
            StringComparer.OrdinalIgnoreCase);

        _tokens = tokens;
    }

    public byte[] Render(PdfTemplateDefinition template, JsonElement data)
    {
        PdfRenderContext? context = null;

        void RenderBlock(IContainer container, PdfBlockDefinition block)
        {
            if (!_blockRenderers.TryGetValue(block.Type, out var renderer))
                throw new InvalidOperationException($"Unsupported block type: {block.Type}");

            renderer.Render(container, block, context!);
        }

        context = new PdfRenderContext(template, data, _tokens, RenderBlock);

        return Document.Create(document =>
        {
            document.Page(page =>
            {
                var pageSize = template.PageSize.ToLowerInvariant() switch
                {
                    "letter" => PageSizes.Letter,
                    _ => PageSizes.A4
                };

                if (string.Equals(
                    template.Orientation,
                    "landscape",
                    StringComparison.OrdinalIgnoreCase))
                {
                    page.Size(pageSize.Landscape());
                }
                else
                {
                    page.Size(pageSize);
                }

                page.Margin(template.Margin);

                page.DefaultTextStyle(x => x.FontSize(template.DefaultFontSize));

                var watermark = template.Blocks.FirstOrDefault(x =>
                    string.Equals(x.Type, "watermark", StringComparison.OrdinalIgnoreCase));

                if (watermark is not null)
                {
                    page.Background()
                        .AlignCenter()
                        .AlignMiddle()
                        .Text(_tokens.Apply(watermark.Text ?? watermark.Value ?? "", data))
                        .FontSize(watermark.FontSize ?? 64)
                        .FontColor(watermark.Color ?? "#E5E7EB")
                        .SemiBold();
                }

                if (!string.IsNullOrWhiteSpace(template.Title))
                {
                    page.Header()
                        .Text(_tokens.Apply(template.Title, data))
                        .SemiBold()
                        .FontSize(16);
                }

                page.Content().Column(column =>
                {
                    column.Spacing(template.BlockSpacing);

                    foreach (var block in template.Blocks)
                    {
                        // Watermark is rendered as page background, not as normal content.
                        if (string.Equals(block.Type, "watermark", StringComparison.OrdinalIgnoreCase))
                            continue;

                        context.RenderBlock(column.Item(), block);
                    }
                });

                if (template.ShowPageNumbers)
                {
                    page.Footer()
                        .AlignCenter()
                        .Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                }
            });
        }).GeneratePdf();
    }
}
