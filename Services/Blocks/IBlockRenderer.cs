using PdfTemplateApi.Models;
using PdfTemplateApi.Services;
using QuestPDF.Infrastructure;

namespace PdfTemplateApi.Services.Blocks;

public interface IBlockRenderer
{
    string Type { get; }

    void Render(
        IContainer container,
        PdfBlockDefinition block,
        PdfRenderContext context);
}
