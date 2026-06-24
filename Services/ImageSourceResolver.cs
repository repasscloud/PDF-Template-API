using System.Text.Json;
using PdfTemplateApi.Models;
using PdfTemplateApi.Services.Blocks;

namespace PdfTemplateApi.Services;

public sealed class ImageSourceResolver
{
    private readonly IWebHostEnvironment _environment;

    public ImageSourceResolver(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<byte[]> ResolveImageBytesAsync(
        PdfBlockDefinition block,
        PdfRenderContext context)
    {
        if (!string.IsNullOrWhiteSpace(block.Base64))
        {
            var base64 = BlockRenderHelpers.ResolveString(block.Base64, context);
            return DecodeBase64Image(base64);
        }

        if (!string.IsNullOrWhiteSpace(block.DataPath) &&
            context.Tokens.TryResolveJsonPath(context.RootData, block.DataPath, out var value))
        {
            if (value.ValueKind == JsonValueKind.String)
            {
                var possibleBase64 = value.GetString();

                if (!string.IsNullOrWhiteSpace(possibleBase64))
                    return DecodeBase64Image(possibleBase64);
            }
        }

        if (!string.IsNullOrWhiteSpace(block.Source))
        {
            var source = BlockRenderHelpers.ResolveString(block.Source, context);

            var assetsRoot = Path.GetFullPath(Path.Combine(
                _environment.ContentRootPath,
                "Assets"));

            var requestedPath = Path.GetFullPath(Path.Combine(
                assetsRoot,
                source.Replace('/', Path.DirectorySeparatorChar)));

            if (!requestedPath.StartsWith(assetsRoot, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Image source path is outside the Assets directory.");

            if (!File.Exists(requestedPath))
                throw new InvalidOperationException($"Image not found: {source}");

            return await File.ReadAllBytesAsync(requestedPath);
        }

        throw new InvalidOperationException("Image block requires base64, dataPath, or source.");
    }

    private static byte[] DecodeBase64Image(string value)
    {
        var commaIndex = value.IndexOf(',');

        if (value.StartsWith("data:image/", StringComparison.OrdinalIgnoreCase) &&
            commaIndex >= 0)
        {
            value = value[(commaIndex + 1)..];
        }

        return Convert.FromBase64String(value);
    }
}
