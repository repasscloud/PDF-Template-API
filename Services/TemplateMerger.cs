using PdfTemplateApi.Models;

namespace PdfTemplateApi.Services;

public sealed class TemplateMerger
{
    public PdfTemplateDefinition ApplyPatch(
        PdfTemplateDefinition template,
        PdfTemplatePatch? patch)
    {
        if (patch is null)
            return template;

        return new PdfTemplateDefinition
        {
            Title = patch.Title ?? template.Title,
            PageSize = patch.PageSize ?? template.PageSize,
            Orientation = patch.Orientation ?? template.Orientation,
            Margin = patch.Margin ?? template.Margin,
            DefaultFontSize = patch.DefaultFontSize ?? template.DefaultFontSize,
            BlockSpacing = patch.BlockSpacing ?? template.BlockSpacing,
            ShowPageNumbers = patch.ShowPageNumbers ?? template.ShowPageNumbers,

            // Deliberately replace the block list if blocks are supplied.
            // This keeps patching simple and predictable.
            Blocks = patch.Blocks ?? template.Blocks
        };
    }
}
