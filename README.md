# PDF Template API

A minimal ASP.NET Core Web API for generating PDFs from JSON templates and JSON data.

The project demonstrates a controlled, template-driven PDF generation approach where the client sends:

- a template name, or a full inline template;
- optional template overrides;
- structured JSON data;
- output metadata such as the requested filename.

The API returns a generated PDF directly from a `POST /pdf` request.

---

## Purpose

This project sets out to create a simpler alternative to Markdown, HTML, or TeX-style PDF generation for common business documents.

Instead of letting a user submit arbitrary rendering code, the system uses a small, predefined set of controlled blocks. Each block is implemented in C#, while JSON only describes which block to use and what data should flow into it.

The core design rule is:

```text
JSON describes intent.
C# owns rendering behaviour.
```

That makes the generator:

- predictable;
- safer than arbitrary HTML or scripting;
- easy to validate;
- easy to version;
- suitable for invoices, statements, certificates, approval documents, and similar business PDFs.

---

## Technology

The project uses:

| Technology | Purpose |
|---|---|
| ASP.NET Core Minimal API | Exposes the `/pdf` endpoint |
| .NET 9 / .NET 10 | Runtime and application framework |
| QuestPDF | PDF document layout and rendering |
| QRCoder | QR code generation |
| ZXing.Net | Barcode generation |
| Custom `PngWriter` | Converts barcode pixel data into PNG without using ImageSharp |
| JSON templates | Defines document structure |
| JSON data payloads | Supplies runtime values for `{{tokens}}` |

The current implementation avoids a direct dependency on `SixLabors.ImageSharp` by using a small custom PNG writer for barcode output.

---

## Project Structure

```text
PdfTemplateApi/
  Program.cs
  PdfTemplateApi.csproj

  Models/
    PdfBuildRequest.cs
    PdfOutputOptions.cs
    PdfTemplateDefinition.cs
    PdfTemplatePatch.cs
    PdfBlockDefinition.cs
    PdfTableColumnDefinition.cs
    PdfKeyValueDefinition.cs

  Services/
    PdfRenderer.cs
    PdfRenderContext.cs
    TemplateStore.cs
    TemplateMerger.cs
    TokenResolver.cs
    ImageSourceResolver.cs
    BarcodeImageGenerator.cs
    PngWriter.cs

    Blocks/
      IBlockRenderer.cs
      BlockRenderHelpers.cs
      HeadingBlockRenderer.cs
      TextBlockRenderer.cs
      SpacerBlockRenderer.cs
      LineBlockRenderer.cs
      TableBlockRenderer.cs
      PageBreakBlockRenderer.cs
      SignatureBlockRenderer.cs
      QrCodeBlockRenderer.cs
      ImageBlockRenderer.cs
      TwoColumnBlockRenderer.cs
      AddressBlockRenderer.cs
      TotalsBoxBlockRenderer.cs
      WatermarkBlockRenderer.cs
      TermsAndConditionsBlockRenderer.cs
      BarcodeBlockRenderer.cs
      CertificateTitleBlockRenderer.cs
      ApprovalStampBlockRenderer.cs

  Templates/
    invoice.json
    invoice2.json

  requests/
    sample-invoice.json
    sample-invoice2.json

  Assets/
    logo.png
```

---

## API Overview

### `GET /`

Health/basic service check.

Example response:

```json
{
  "service": "PDF Template API",
  "endpoints": [
    "POST /pdf"
  ]
}
```

### `POST /pdf`

Generates a PDF from a template and JSON data.

The endpoint accepts a request body like:

```json
{
  "templateName": "invoice2",
  "output": {
    "fileName": "invoice2-1002.pdf"
  },
  "templatePatch": {
    "margin": 36,
    "title": "Tax Invoice {{invoice.number}}"
  },
  "data": {
    "invoice": {
      "number": "INV-1002",
      "date": "24 June 2026"
    }
  }
}
```

The endpoint returns:

```text
Content-Type: application/pdf
```

---

## Request Model

### `PdfBuildRequest`

| Property | Type | Purpose |
|---|---|---|
| `templateName` | string | Loads a JSON template from `/Templates/{templateName}.json` |
| `template` | object | Optional full inline template. If supplied, this replaces `templateName` loading |
| `templatePatch` | object | Optional override applied after the template is loaded |
| `data` | object | JSON data used by `{{tokens}}` |
| `output.fileName` | string | Suggested PDF download filename |

Example:

```json
{
  "templateName": "invoice",
  "output": {
    "fileName": "invoice-1001.pdf"
  },
  "data": {
    "invoice": {
      "number": "INV-1001"
    }
  }
}
```

---

## Template Model

A template controls page-level document settings and a list of blocks.

```json
{
  "title": "Tax Invoice {{invoice.number}}",
  "pageSize": "A4",
  "orientation": "Portrait",
  "margin": 40,
  "defaultFontSize": 10,
  "blockSpacing": 8,
  "showPageNumbers": true,
  "blocks": []
}
```

### Template Properties

| Property | Purpose |
|---|---|
| `title` | Optional page header text. Supports tokens |
| `pageSize` | Supported sample values: `A4`, `Letter` |
| `orientation` | `Portrait` or `Landscape` |
| `margin` | Page margin in points |
| `defaultFontSize` | Default font size for text blocks |
| `blockSpacing` | Vertical spacing between blocks |
| `showPageNumbers` | Adds a page footer with current and total page numbers |
| `blocks` | Ordered list of controlled content blocks |

---

## Token Syntax

Templates use `{{path.to.value}}` tokens.

Example:

```json
{
  "text": "Invoice Number: {{invoice.number}}"
}
```

Given this data:

```json
{
  "invoice": {
    "number": "INV-1001"
  }
}
```

The rendered text becomes:

```text
Invoice Number: INV-1001
```

Token resolution is case-insensitive for property names.

### Root Data Tokens

These resolve from the full request `data` object:

```text
{{invoice.number}}
{{customer.name}}
{{totals.total}}
```

### Local Row Tokens

Inside a table using `dataPath`, tokens resolve against the current row first.

Example table:

```json
{
  "type": "table",
  "dataPath": "items",
  "columns": [
    {
      "header": "Description",
      "value": "{{description}}"
    },
    {
      "header": "Total",
      "value": "{{total}}"
    }
  ]
}
```

Given:

```json
{
  "items": [
    {
      "description": "Platform licence",
      "total": "$499.00"
    }
  ]
}
```

The table row resolves from each item in the array.

---

## Controlled Blocks

The current block system supports these types:

```text
heading
text
spacer
line
table
pageBreak
signature
qrCode
image
twoColumn
addressBlock
totalsBox
watermark
termsAndConditions
barcode
certificateTitle
approvalStamp
```

Each block has a dedicated C# renderer implementing:

```csharp
public interface IBlockRenderer
{
    string Type { get; }

    void Render(
        IContainer container,
        PdfBlockDefinition block,
        PdfRenderContext context);
}
```

---

## Block Reference

### `heading`

Renders a heading.

```json
{
  "type": "heading",
  "level": 1,
  "text": "Tax Invoice"
}
```

Supported fields:

| Field | Purpose |
|---|---|
| `text` | Heading text |
| `level` | Heading level; affects default size |
| `fontSize` | Optional explicit font size |

---

### `text`

Renders a text line or paragraph.

```json
{
  "type": "text",
  "bold": true,
  "text": "Total: {{totals.total}}",
  "align": "right"
}
```

Supported fields:

| Field | Purpose |
|---|---|
| `text` | Text content |
| `fontSize` | Optional font size |
| `bold` | Makes text semi-bold |
| `italic` | Makes text italic |
| `align` | `left`, `center`, `centre`, or `right` |

---

### `spacer`

Adds vertical spacing.

```json
{
  "type": "spacer",
  "height": 12
}
```

---

### `line`

Adds a horizontal divider.

```json
{
  "type": "line"
}
```

---

### `table`

Renders a table from an array in `data`, or from static rows.

```json
{
  "type": "table",
  "dataPath": "items",
  "showHeader": true,
  "columns": [
    {
      "header": "Description",
      "value": "{{description}}"
    },
    {
      "header": "Total",
      "value": "{{total}}",
      "width": 80,
      "align": "right"
    }
  ]
}
```

Supported fields:

| Field | Purpose |
|---|---|
| `dataPath` | Path to an array in the request data |
| `showHeader` | Shows or hides the header row |
| `columns` | Column definitions |
| `rows` | Optional static rows |

Column fields:

| Field | Purpose |
|---|---|
| `header` | Column heading |
| `value` | Token/literal used for each row |
| `width` | Optional fixed width |
| `align` | `left`, `center`, `right` |

---

### `pageBreak`

Forces a new page.

```json
{
  "type": "pageBreak"
}
```

---

### `signature`

Renders a signature area with a line, label, optional name, and optional caption.

```json
{
  "type": "signature",
  "label": "Customer Signature",
  "value": "{{customer.name}}",
  "caption": "Date: {{invoice.date}}",
  "width": 260,
  "height": 50,
  "align": "left"
}
```

---

### `qrCode`

Renders a QR code.

```json
{
  "type": "qrCode",
  "value": "{{invoice.paymentUrl}}",
  "size": 90,
  "caption": "Scan to pay",
  "align": "right"
}
```

Supported fields:

| Field | Purpose |
|---|---|
| `value` | QR payload |
| `text` | Alternative QR payload field |
| `dataPath` | Alternative path to payload in data |
| `size` | Width/height |
| `caption` | Optional caption |
| `align` | Alignment |

---

### `image`

Renders an image from `/Assets` or from base64 data.

From `/Assets/logo.png`:

```json
{
  "type": "image",
  "source": "logo.png",
  "width": 120,
  "align": "left"
}
```

From base64:

```json
{
  "type": "image",
  "base64": "{{company.logoBase64}}",
  "width": 120,
  "align": "left"
}
```

Supported fields:

| Field | Purpose |
|---|---|
| `source` | File under `/Assets` |
| `base64` | Base64 image data or data URI |
| `dataPath` | Path to base64 image data |
| `width` | Optional image width |
| `height` | Optional image height |
| `align` | Alignment |

Security note: file images are constrained to the `/Assets` directory.

---

### `twoColumn`

Renders two side-by-side block groups.

```json
{
  "type": "twoColumn",
  "gap": 24,
  "leftWidth": 1,
  "rightWidth": 1,
  "leftBlocks": [
    {
      "type": "addressBlock",
      "label": "Bill To",
      "lines": [
        "{{customer.name}}",
        "{{customer.email}}"
      ]
    }
  ],
  "rightBlocks": [
    {
      "type": "qrCode",
      "value": "{{invoice.paymentUrl}}",
      "size": 90,
      "align": "right"
    }
  ]
}
```

Supported fields:

| Field | Purpose |
|---|---|
| `leftBlocks` | Blocks rendered in the left column |
| `rightBlocks` | Blocks rendered in the right column |
| `gap` | Fixed space between columns |
| `leftWidth` | Relative left column width |
| `rightWidth` | Relative right column width |

---

### `addressBlock`

Renders a labelled address/contact block.

```json
{
  "type": "addressBlock",
  "label": "Customer",
  "lines": [
    "{{customer.name}}",
    "{{customer.email}}",
    "{{customer.address.line1}}",
    "{{customer.address.city}} {{customer.address.state}} {{customer.address.postcode}}"
  ]
}
```

Blank lines are skipped.

---

### `totalsBox`

Renders invoice totals as a compact right-aligned key/value box.

```json
{
  "type": "totalsBox",
  "align": "right",
  "width": 250,
  "padding": 10,
  "keyValues": [
    {
      "label": "Subtotal",
      "value": "{{totals.subtotal}}"
    },
    {
      "label": "GST",
      "value": "{{totals.gst}}"
    },
    {
      "label": "Total",
      "value": "{{totals.total}}",
      "bold": true,
      "fontSize": 13
    }
  ]
}
```

This is preferred over using separate right-aligned `text` blocks for invoice totals.

---

### `watermark`

Renders a page-level background watermark.

```json
{
  "type": "watermark",
  "text": "DRAFT",
  "fontSize": 72,
  "color": "#EEEEEE"
}
```

Watermark blocks are handled by `PdfRenderer` as page background content, not normal flowing body content.

---

### `termsAndConditions`

Renders terms as a title and numbered lines.

```json
{
  "type": "termsAndConditions",
  "label": "Terms and Conditions",
  "lines": [
    "Payment is due by {{invoice.dueDate}}.",
    "All prices are in AUD unless otherwise stated.",
    "Late payment may result in service suspension."
  ]
}
```

---

### `barcode`

Renders a barcode.

```json
{
  "type": "barcode",
  "label": "CODE_128",
  "value": "{{invoice.number}}",
  "width": 260,
  "height": 70,
  "caption": "{{invoice.number}}",
  "align": "right"
}
```

Supported sample formats:

```text
CODE_128
CODE_39
CODE_93
EAN_8
EAN_13
UPC_A
UPC_E
PDF_417
DATA_MATRIX
AZTEC
QR
QRCODE
```

---

### `certificateTitle`

Renders a centred certificate-style title.

```json
{
  "type": "certificateTitle",
  "text": "Certificate of Completion",
  "caption": "Presented to {{customer.name}}",
  "fontSize": 30
}
```

---

### `approvalStamp`

Renders an approval/status stamp.

```json
{
  "type": "approvalStamp",
  "status": "APPROVED",
  "value": "Approved by {{approval.name}}",
  "caption": "{{approval.date}}",
  "align": "right",
  "width": 190
}
```

---

## Template Selection

### Named template

```json
{
  "templateName": "invoice2",
  "data": {}
}
```

This loads:

```text
Templates/invoice2.json
```

### Inline template

```json
{
  "template": {
    "title": "Inline Document",
    "pageSize": "A4",
    "blocks": [
      {
        "type": "heading",
        "text": "Hello"
      }
    ]
  },
  "data": {}
}
```

When `template` is supplied, it replaces `templateName` loading.

### Template patch

```json
{
  "templateName": "invoice2",
  "templatePatch": {
    "margin": 36,
    "title": "Tax Invoice {{invoice.number}}"
  },
  "data": {}
}
```

The current simple patch model supports replacing page-level settings and replacing the entire `blocks` list if `blocks` is supplied.

---

## Running the API

```bash
dotnet run --urls http://localhost:5122
```

Open:

```text
http://localhost:5122/
```

---

## Generate Sample PDFs

### Generate `invoice-1001.pdf`

```bash
curl --fail --silent --show-error \
  -X POST http://localhost:5122/pdf \
  -H "Content-Type: application/json" \
  --data-binary "@requests/sample-invoice.json" \
  --output "invoice-1001.pdf"
```

### Generate `invoice2-1002.pdf`

```bash
curl --fail --silent --show-error \
  -X POST http://localhost:5122/pdf \
  -H "Content-Type: application/json" \
  --data-binary "@requests/sample-invoice2.json" \
  --output "invoice2-1002.pdf"
```

---

## Test Script

Example `test-invoices.sh`:

```bash
#!/usr/bin/env bash
set -euo pipefail

URL="http://localhost:5122"
LOG_FILE="app.log"
PID_FILE="app.pid"

cleanup() {
  if [[ -f "$PID_FILE" ]]; then
    PID="$(cat "$PID_FILE")"

    if kill -0 "$PID" 2>/dev/null; then
      kill "$PID" 2>/dev/null || true
    fi

    rm -f "$PID_FILE"
  fi
}

trap cleanup EXIT

echo "Starting API on $URL..."

dotnet run --urls "$URL" > "$LOG_FILE" 2>&1 &
APP_PID=$!

echo "$APP_PID" > "$PID_FILE"

echo "PID: $APP_PID"
echo "Waiting for API to start..."

for i in {1..30}; do
  if curl --silent --fail "$URL/" > /dev/null 2>&1; then
    echo "API is ready."
    break
  fi

  if ! kill -0 "$APP_PID" 2>/dev/null; then
    echo "API process exited before becoming ready."
    echo "---- app.log ----"
    cat "$LOG_FILE"
    exit 1
  fi

  sleep 1

  if [[ "$i" -eq 30 ]]; then
    echo "API did not start within 30 seconds."
    echo "---- app.log ----"
    cat "$LOG_FILE"
    exit 1
  fi
done

echo "Generating invoice-1001.pdf..."

curl --fail --silent --show-error \
  -X POST "$URL/pdf" \
  -H "Content-Type: application/json" \
  --data-binary "@requests/sample-invoice.json" \
  --output "invoice-1001.pdf"

echo "Created invoice-1001.pdf"

echo "Generating invoice2-1002.pdf..."

curl --fail --silent --show-error \
  -X POST "$URL/pdf" \
  -H "Content-Type: application/json" \
  --data-binary "@requests/sample-invoice2.json" \
  --output "invoice2-1002.pdf"

echo "Created invoice2-1002.pdf"

echo "Done."
```

Run it:

```bash
chmod +x test-invoices.sh
./test-invoices.sh
```

---

## Adding a New Block

To add a controlled block:

1. Add properties to `PdfBlockDefinition` if needed.
2. Create a new renderer implementing `IBlockRenderer`.
3. Register it in `Program.cs`.
4. Add JSON examples to a template.

Example renderer skeleton:

```csharp
using PdfTemplateApi.Models;
using QuestPDF.Infrastructure;

namespace PdfTemplateApi.Services.Blocks;

public sealed class MyBlockRenderer : IBlockRenderer
{
    public string Type => "myBlock";

    public void Render(
        IContainer container,
        PdfBlockDefinition block,
        PdfRenderContext context)
    {
        // Controlled rendering logic goes here.
    }
}
```

Register it:

```csharp
builder.Services.AddSingleton<IBlockRenderer, MyBlockRenderer>();
```

Use it:

```json
{
  "type": "myBlock"
}
```

---

## ImageSharp Branch Idea

The main branch can avoid `SixLabors.ImageSharp` by using the custom `PngWriter`.

A separate development branch could be created to test an ImageSharp-based implementation for image processing and barcode/QR pipelines.

Suggested branch name:

```text
feature/imagesharp-rendering-pipeline
```

Potential use cases for the ImageSharp branch:

- resizing uploaded images;
- validating image dimensions;
- converting image formats;
- normalising logos before embedding into PDFs;
- replacing the custom `PngWriter` with a library-backed PNG encoder;
- experimenting with image compression and quality settings.

Keep the branch clearly marked as experimental unless the project has a valid license for the relevant usage.

---

## Limitations

The current implementation is intentionally small.

Known limitations:

- no full JSON Schema validation yet;
- no advanced patching of individual blocks by ID;
- limited page-size support;
- limited styling options;
- no authentication or authorization;
- no persistent document storage;
- no template versioning database;
- no async rendering pipeline;
- no queue/background worker;
- no HTML or Markdown rendering;
- no built-in digital signatures;
- no PDF/A compliance mode.

---

## Possible Next Improvements

Good next steps:

- add JSON Schema validation for templates;
- add `id` to each block so templates can be patched block-by-block;
- add template versioning: `invoice.v1`, `invoice.v2`, etc.;
- add a `/templates` endpoint to list available templates;
- add a `/templates/{name}/validate` endpoint;
- add a `/pdf/preview` endpoint that returns debug info;
- add support for custom fonts;
- add tenant/company-specific templates;
- add Azure Blob/S3 publishing;
- add email delivery;
- add automated PDF snapshot tests;
- add OpenAPI/Swagger for request examples.

---

## Production Notes

Before using this in production, add:

- request size limits;
- input validation;
- authentication;
- template allow-listing;
- output logging;
- correlation IDs;
- structured error responses;
- rate limiting;
- secure image handling;
- tests for token resolution;
- tests for every block renderer;
- versioned templates;
- license review for all third-party packages.

---

## Summary

This project provides a controlled JSON-to-PDF pipeline for C# applications.

It is designed for scenarios where documents need to be generated dynamically, but where full HTML, Markdown, or TeX-level rendering would be unnecessarily flexible or unsafe.

The system is intentionally constrained:

```text
Templates define document intent.
Data supplies runtime values.
C# renderers control the final PDF output.
```
