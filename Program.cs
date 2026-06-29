using System.Text.Json;
using PdfTemplateApi.Models;
using PdfTemplateApi.Services;
using PdfTemplateApi.Services.Blocks;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Required by QuestPDF.
// Pick the license type that matches your actual usage.
QuestPDF.Settings.License = LicenseType.Community;

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.WriteIndented = true;
});

builder.Services.AddSingleton(new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
});

builder.Services.AddSingleton<TokenResolver>();
builder.Services.AddSingleton<TemplateStore>();
builder.Services.AddSingleton<TemplateMerger>();
builder.Services.AddSingleton<PdfRenderer>();
builder.Services.AddSingleton<ImageSourceResolver>();
builder.Services.AddSingleton<BarcodeImageGenerator>();
builder.Services.AddSingleton<PdfStamper>();

builder.Services.AddSingleton<IBlockRenderer, HeadingBlockRenderer>();
builder.Services.AddSingleton<IBlockRenderer, TextBlockRenderer>();
builder.Services.AddSingleton<IBlockRenderer, SpacerBlockRenderer>();
builder.Services.AddSingleton<IBlockRenderer, LineBlockRenderer>();
builder.Services.AddSingleton<IBlockRenderer, TableBlockRenderer>();
builder.Services.AddSingleton<IBlockRenderer, PageBreakBlockRenderer>();

builder.Services.AddSingleton<IBlockRenderer, SignatureBlockRenderer>();
builder.Services.AddSingleton<IBlockRenderer, QrCodeBlockRenderer>();
builder.Services.AddSingleton<IBlockRenderer, ImageBlockRenderer>();
builder.Services.AddSingleton<IBlockRenderer, TwoColumnBlockRenderer>();
builder.Services.AddSingleton<IBlockRenderer, AddressBlockRenderer>();
builder.Services.AddSingleton<IBlockRenderer, TotalsBoxBlockRenderer>();
builder.Services.AddSingleton<IBlockRenderer, WatermarkBlockRenderer>();
builder.Services.AddSingleton<IBlockRenderer, TermsAndConditionsBlockRenderer>();
builder.Services.AddSingleton<IBlockRenderer, BarcodeBlockRenderer>();
builder.Services.AddSingleton<IBlockRenderer, CertificateTitleBlockRenderer>();
builder.Services.AddSingleton<IBlockRenderer, ApprovalStampBlockRenderer>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    service = "PDF Template API",
    endpoints = new[]
    {
        "POST /pdf",
        "POST /pdf/stamp"
    }
}));

app.MapPost("/pdf", async (
    PdfBuildRequest request,
    TemplateStore templateStore,
    TemplateMerger templateMerger,
    PdfRenderer pdfRenderer) =>
{
    try
    {
        var template = await templateStore.ResolveTemplateAsync(request);
        template = templateMerger.ApplyPatch(template, request.TemplatePatch);

        var pdfBytes = pdfRenderer.Render(template, request.Data);

        var fileName = string.IsNullOrWhiteSpace(request.Output?.FileName)
            ? "document.pdf"
            : request.Output.FileName;

        return Results.File(
            fileContents: pdfBytes,
            contentType: "application/pdf",
            fileDownloadName: fileName);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new
        {
            error = ex.Message
        });
    }
});

app.MapPost("/pdf/stamp", (PdfStampRequest request, PdfStamper stamper) =>
{
    try
    {
        var stampedBytes = stamper.ApplyStamp(request);

        var fileName = string.IsNullOrWhiteSpace(request.FileName)
            ? "stamped.pdf"
            : request.FileName;

        return Results.File(
            fileContents: stampedBytes,
            contentType: "application/pdf",
            fileDownloadName: fileName);
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.Run();
