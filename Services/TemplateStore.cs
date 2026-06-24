using System.Text.Json;
using PdfTemplateApi.Models;

namespace PdfTemplateApi.Services;

public sealed class TemplateStore
{
    private readonly IWebHostEnvironment _environment;
    private readonly JsonSerializerOptions _jsonOptions;

    public TemplateStore(
        IWebHostEnvironment environment,
        JsonSerializerOptions jsonOptions)
    {
        _environment = environment;
        _jsonOptions = jsonOptions;
    }

    public async Task<PdfTemplateDefinition> ResolveTemplateAsync(PdfBuildRequest request)
    {
        if (request.Template is not null)
        {
            ValidateTemplate(request.Template);
            return request.Template;
        }

        var templateName = string.IsNullOrWhiteSpace(request.TemplateName)
            ? "invoice"
            : request.TemplateName;

        var safeTemplateName = Path.GetFileNameWithoutExtension(templateName);
        var templatePath = Path.Combine(
            _environment.ContentRootPath,
            "Templates",
            $"{safeTemplateName}.json");

        if (!File.Exists(templatePath))
            throw new InvalidOperationException($"Template not found: {safeTemplateName}");

        var json = await File.ReadAllTextAsync(templatePath);

        var template = JsonSerializer.Deserialize<PdfTemplateDefinition>(
            json,
            _jsonOptions);

        if (template is null)
            throw new InvalidOperationException($"Template could not be parsed: {safeTemplateName}");

        ValidateTemplate(template);

        return template;
    }

    private static void ValidateTemplate(PdfTemplateDefinition template)
    {
        if (template.Blocks.Count == 0)
            throw new InvalidOperationException("Template must contain at least one block.");

        foreach (var block in template.Blocks)
        {
            if (string.IsNullOrWhiteSpace(block.Type))
                throw new InvalidOperationException("Every block must have a type.");
        }
    }
}
