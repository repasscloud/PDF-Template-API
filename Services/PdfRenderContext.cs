using System.Text.Json;
using PdfTemplateApi.Models;
using QuestPDF.Infrastructure;

namespace PdfTemplateApi.Services;

public sealed class PdfRenderContext
{
    public PdfRenderContext(
        PdfTemplateDefinition template,
        JsonElement rootData,
        TokenResolver tokens,
        Action<IContainer, PdfBlockDefinition> renderBlock)
    {
        Template = template;
        RootData = rootData;
        Tokens = tokens;
        RenderBlock = renderBlock;
    }

    public PdfTemplateDefinition Template { get; }

    public JsonElement RootData { get; }

    public TokenResolver Tokens { get; }

    public Action<IContainer, PdfBlockDefinition> RenderBlock { get; }
}
