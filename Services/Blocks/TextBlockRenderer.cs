using PdfTemplateApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PdfTemplateApi.Services.Blocks;

public sealed class TextBlockRenderer : IBlockRenderer
{
    public string Type => "text";

    public void Render(
        IContainer container,
        PdfBlockDefinition block,
        PdfRenderContext context)
    {
        var text = context.Tokens.Apply(
            block.Text ?? string.Empty,
            context.RootData);

        var fontSize = block.FontSize ?? context.Template.DefaultFontSize;

        var alignedContainer = ApplyAlignment(container, block.Align);

        alignedContainer.Text(textDescriptor =>
        {
            var span = textDescriptor.Span(text).FontSize(fontSize);

            if (block.Bold == true)
                span.SemiBold();

            if (block.Italic == true)
                span.Italic();
        });
    }

    private static IContainer ApplyAlignment(IContainer container, string? align)
    {
        return align?.ToLowerInvariant() switch
        {
            "right" => container.AlignRight(),
            "center" => container.AlignCenter(),
            "centre" => container.AlignCenter(),
            "left" => container.AlignLeft(),
            _ => container
        };
    }
}