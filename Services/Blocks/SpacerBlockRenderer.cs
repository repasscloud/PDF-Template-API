using PdfTemplateApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PdfTemplateApi.Services.Blocks;

public sealed class SpacerBlockRenderer : IBlockRenderer
{
    public string Type => "spacer";

    public void Render(
        IContainer container,
        PdfBlockDefinition block,
        PdfRenderContext context)
    {
        container.Height(block.Height ?? 10);
    }
}
