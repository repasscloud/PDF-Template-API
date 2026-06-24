using PdfTemplateApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PdfTemplateApi.Services.Blocks;

public sealed class LineBlockRenderer : IBlockRenderer
{
    public string Type => "line";

    public void Render(
        IContainer container,
        PdfBlockDefinition block,
        PdfRenderContext context)
    {
        container.LineHorizontal(1);
    }
}
