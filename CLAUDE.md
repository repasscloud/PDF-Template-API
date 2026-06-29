# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

A minimal ASP.NET Core 10 Web API that generates PDFs from JSON templates. The single endpoint `POST /pdf` accepts a request body, resolves a template, applies an optional patch, substitutes `{{token}}` placeholders with data, and returns a PDF binary via QuestPDF.

## Commands

```bash
# Build
dotnet build --configuration Release

# Run (default port: http://localhost:5122)
dotnet run

# End-to-end integration test — starts the API, generates all PDFs, shuts down
./test-invoices.sh

# Send a single request manually
curl -X POST http://localhost:5122/pdf \
  -H "Content-Type: application/json" \
  --data-binary "@requests/sample-invoice.json" \
  --output invoice-1001.pdf
```

There are no unit tests; correctness is verified by `test-invoices.sh` which produces PDF files as output artifacts. CI runs this script on every push/PR via `.github/workflows/dotnet.yml`.

## Architecture

### Request flow

```
POST /pdf (Program.cs)
  → TemplateStore.ResolveTemplateAsync()   # load from /Templates/*.json or inline
  → TemplateMerger.ApplyPatch()            # optional partial override
  → PdfRenderer.Render()                   # QuestPDF document assembly
       └─ per block → IBlockRenderer.Render()
```

### Key components

| Component | Purpose |
|---|---|
| `Models/PdfBuildRequest` | Top-level request: `templateName`, inline `template`, `templatePatch`, `data`, `output` |
| `Models/PdfTemplateDefinition` | Template schema: page size/orientation/margin, font size, block spacing, block list |
| `Models/PdfBlockDefinition` | Flat block descriptor — all possible fields for all block types on one class |
| `Services/TemplateStore` | Loads `/Templates/{name}.json`; validates blocks non-empty and all have `type` |
| `Services/TemplateMerger` | Shallow-merges a `PdfTemplatePatch` onto a template; replaces the entire block list when `blocks` is supplied |
| `Services/TokenResolver` | Replaces `{{path}}` / `{{$.path}}` tokens with values from `JsonElement data`; resolves dot-separated paths and array indexes; leaves unresolved tokens visible |
| `Services/PdfRenderer` | QuestPDF document builder; dispatches each block to a registered `IBlockRenderer`; handles page header (title), footer (page numbers), and watermark as page background |
| `Services/PdfRenderContext` | Passed to every block renderer: holds template, root `JsonElement`, `TokenResolver`, and a recursive `RenderBlock` delegate |
| `Services/Blocks/IBlockRenderer` | Interface: `string Type` + `Render(IContainer, PdfBlockDefinition, PdfRenderContext)` |

### Adding a new block type

1. Create `Services/Blocks/MyNewBlockRenderer.cs` implementing `IBlockRenderer`.
2. Register it in `Program.cs` as `builder.Services.AddSingleton<IBlockRenderer, MyNewBlockRenderer>()`.
3. The `PdfRenderer` discovers it automatically via the `IEnumerable<IBlockRenderer>` constructor parameter.

### Table rendering

Tables support two data modes (checked in order):

- **`dataPath`** — dot-path into `data` resolving to a JSON array; column `value` fields are token-template strings evaluated per row with the row element as `localData`.
- **`rows`** — static `string[][]` with token strings resolved against root `data` only.

### Token syntax

- `{{fieldName}}` — root data property (case-insensitive)
- `{{parent.child}}` — nested path
- `{{items.0.name}}` — array index access
- `{{$.path}}` or `{{$path}}` — `$` prefix is stripped, same resolution

### Templates

JSON files in `/Templates/` are the default template library. The `templateName` request field maps to `{name}.json` (extension stripped via `Path.GetFileNameWithoutExtension` to prevent path traversal). Sample request payloads live in `/requests/`.

### QuestPDF license

Set to `LicenseType.Community` in `Program.cs`. Change this if the deployment exceeds community licence limits.
