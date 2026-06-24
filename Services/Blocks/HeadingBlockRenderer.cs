using PdfTemplateApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PdfTemplateApi.Services.Blocks;

public sealed class HeadingBlockRenderer : IBlockRenderer
{
    public string Type => "heading";

    public void Render(
        IContainer container,
        PdfBlockDefinition block,
        PdfRenderContext context)
    {
        var text = context.Tokens.Apply(
            block.Text ?? string.Empty,
            context.RootData);

        var level = block.Level ?? 1;

        var fontSize = block.FontSize ?? level switch
        {
            1 => 20,
            2 => 16,
            3 => 13,
            _ => 11
        };

        container
            .Text(text)
            .SemiBold()
            .FontSize(fontSize);
    }
}
