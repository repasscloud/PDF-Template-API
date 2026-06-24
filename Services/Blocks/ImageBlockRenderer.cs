using PdfTemplateApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PdfTemplateApi.Services.Blocks;

public sealed class ImageBlockRenderer : IBlockRenderer
{
    private readonly ImageSourceResolver _images;

    public ImageBlockRenderer(ImageSourceResolver images)
    {
        _images = images;
    }

    public string Type => "image";

    public void Render(
        IContainer container,
        PdfBlockDefinition block,
        PdfRenderContext context)
    {
        var bytes = _images
            .ResolveImageBytesAsync(block, context)
            .GetAwaiter()
            .GetResult();

        var aligned = BlockRenderHelpers.ApplyAlignment(container, block.Align);

        if (block.Width is > 0 && block.Height is > 0)
        {
            aligned.Width(block.Width.Value)
                .Height(block.Height.Value)
                .Image(bytes)
                .FitArea();

            return;
        }

        if (block.Width is > 0)
        {
            aligned.Width(block.Width.Value)
                .Image(bytes)
                .FitWidth();

            return;
        }

        if (block.Height is > 0)
        {
            aligned.Height(block.Height.Value)
                .Image(bytes)
                .FitHeight();

            return;
        }

        aligned.Image(bytes)
            .FitWidth();
    }
}
