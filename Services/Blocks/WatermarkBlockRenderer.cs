using PdfTemplateApi.Models;
using QuestPDF.Infrastructure;

namespace PdfTemplateApi.Services.Blocks;

public sealed class WatermarkBlockRenderer : IBlockRenderer
{
    public string Type => "watermark";

    public void Render(
        IContainer container,
        PdfBlockDefinition block,
        PdfRenderContext context)
    {
        // Intentionally empty.
        // Watermarks are rendered at page background level by PdfRenderer.
    }
}
