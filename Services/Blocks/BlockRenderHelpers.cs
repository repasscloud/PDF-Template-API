using System.Text.Json;
using PdfTemplateApi.Models;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace PdfTemplateApi.Services.Blocks;

public static class BlockRenderHelpers
{
    public static IContainer ApplyAlignment(IContainer container, string? align)
    {
        return align?.ToLowerInvariant() switch
        {
            "right" => container.AlignRight(),
            "center" => container.AlignCenter(),
            "centre" => container.AlignCenter(),
            "middle" => container.AlignCenter(),
            "left" => container.AlignLeft(),
            _ => container
        };
    }

    public static string ResolveValue(
        PdfBlockDefinition block,
        PdfRenderContext context,
        string fallback = "")
    {
        if (!string.IsNullOrWhiteSpace(block.Value))
            return context.Tokens.Apply(block.Value, context.RootData);

        if (!string.IsNullOrWhiteSpace(block.Text))
            return context.Tokens.Apply(block.Text, context.RootData);

        if (!string.IsNullOrWhiteSpace(block.DataPath) &&
            context.Tokens.TryResolveJsonPath(context.RootData, block.DataPath, out var value))
        {
            return JsonToString(value);
        }

        return fallback;
    }

    public static string ResolveString(
        string? value,
        PdfRenderContext context)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return context.Tokens.Apply(value, context.RootData);
    }

    public static void RenderStyledText(
        IContainer container,
        string text,
        float fontSize,
        bool bold = false,
        bool italic = false,
        string? color = null)
    {
        container.Text(descriptor =>
        {
            var span = descriptor.Span(text).FontSize(fontSize);

            if (bold)
                span.SemiBold();

            if (italic)
                span.Italic();

            if (!string.IsNullOrWhiteSpace(color))
                span.FontColor(color);
        });
    }

    private static string JsonToString(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number => element.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            JsonValueKind.Null => string.Empty,
            JsonValueKind.Undefined => string.Empty,
            _ => element.GetRawText()
        };
    }
}
