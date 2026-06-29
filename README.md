# PDF Template API

A lightweight ASP.NET Core 10 JSON-driven PDF generation service. You describe a document as a JSON template, post your data, and get a print-ready PDF back. No Word, no Puppeteer, no headless browser — just a fast HTTP endpoint.

Built on [QuestPDF](https://www.questpdf.com/) (Community licence), [iText7](https://itextpdf.com/) (stamp overlay), [QRCoder](https://github.com/codebude/QRCoder) and [ZXing.Net](https://github.com/micjahn/ZXing.Net).

---

## Quick Start

```bash
dotnet run

# Health check
curl http://localhost:5122/

# Generate a PDF from a named template + data
curl -X POST http://localhost:5122/pdf \
  -H "Content-Type: application/json" \
  --data-binary "@requests/sample-invoice2.json" \
  --output invoice.pdf

# Run the full test suite (starts API, generates all sample PDFs, shuts down)
bash test-invoices.sh
```

---

## Endpoints

### `GET /`
Returns a service description and the list of available endpoints.

---

### `POST /pdf`
Generates a PDF and returns it as `application/pdf`.

**Request body (JSON):**

| Field | Type | Description |
|---|---|---|
| `templateName` | string | Name of a JSON file in `/Templates/` (without `.json`). Default: `"invoice"`. |
| `template` | object | Inline `PdfTemplateDefinition`. Overrides `templateName` entirely. |
| `templatePatch` | object | Partial override applied on top of the loaded template (page size, margin, block list, etc.). |
| `data` | object | Arbitrary JSON — values are injected into `{{token}}` placeholders in blocks. |
| `output.fileName` | string | Filename sent in `Content-Disposition`. Default: `"document.pdf"`. |

**Priority:** `template` (inline) → `templateName` (file) → default `invoice` template.
`templatePatch` is always applied last, after whichever template was selected.

---

### `POST /pdf/stamp`
Overlays a coloured approval stamp on an existing PDF and returns the result as `application/pdf`.

**Request body (JSON):**

| Field | Type | Default | Description |
|---|---|---|---|
| `pdfBase64` | string | **required** | Base64-encoded bytes of the PDF to stamp. |
| `status` | string | `"APPROVED"` | Large bold text on the stamp (e.g. `"APPROVED"`, `"REJECTED"`, `"PAID"`, `"VERIFIED"`). |
| `value` | string | — | Optional second line (e.g. `"Approved by Finance"`). |
| `caption` | string | — | Optional third line — typically a date. |
| `color` | string | `"#166534"` | Hex colour for the border and all text. |
| `position` | string | `"topRight"` | Named corner — ignored when `x` and `y` are provided. |
| `x` | number | — | Exact X of the stamp's left edge in PDF points. Takes priority over `position`. |
| `y` | number | — | Exact Y of the stamp's bottom edge in PDF points. Takes priority over `position`. |
| `width` | number | `180` | Stamp box width in points. |
| `allPages` | boolean | `true` | Stamp every page. Set `false` for first page only. |
| `fileName` | string | `"stamped.pdf"` | Content-Disposition filename. |

**Named positions** (when `x`/`y` are omitted):

| Value | Placement |
|---|---|
| `topRight` | 28 pt from top-right corner |
| `topLeft` | 28 pt from top-left corner |
| `bottomRight` | 28 pt from bottom-right corner |
| `bottomLeft` | 28 pt from bottom-left corner |
| `center` | Centred on page |

**PDF coordinate system** — origin `(0, 0)` is at the **bottom-left** of the page. Y increases **upward**.

Common page sizes in points (1 pt = 1/72 inch ≈ 0.353 mm, 1 cm ≈ 28.35 pt):

| Page | Width | Height |
|---|---|---|
| A4 Portrait | 595 pt | 842 pt |
| A4 Landscape | 842 pt | 595 pt |
| Letter Portrait | 612 pt | 792 pt |

To place a stamp 28 pt inside the top-right corner of A4 portrait (stamp ≈ 125 × 84 pt):
- `x = 595 − 28 − 125 = 442`
- `y = 842 − 28 − 84  = 730`

**Stamp colour reference:**

| Colour | Hex |
|---|---|
| Green (approved) | `#166534` |
| Red (rejected) | `#991B1B` |
| Orange (pending) | `#C2500A` |
| Gold | `#8A6D1D` |
| Navy | `#1A3A5C` |
| Deep pink | `#9D174D` |

---

## Template Definition

A template (`PdfTemplateDefinition`) describes the page and the ordered list of content blocks.

| Field | Type | Default | Description |
|---|---|---|---|
| `title` | string | `""` | Page header text (top-left on every page). Leave empty to suppress. |
| `pageSize` | string | `"A4"` | `"A4"` or `"Letter"`. |
| `orientation` | string | `"Portrait"` | `"Portrait"` or `"Landscape"`. |
| `margin` | number | `40` | Page margin in points applied to all four sides. |
| `defaultFontSize` | number | `10` | Fallback font size for all blocks. |
| `blockSpacing` | number | `8` | Vertical gap between top-level blocks (pt). |
| `showPageNumbers` | boolean | `true` | Renders "Page N of M" in the footer. |
| `blocks` | array | **required** | Ordered list of block definitions. |

---

## Block Types

Every block has a `"type"` field. Additional fields vary by type. All text fields support `{{token}}` syntax.

---

### `heading`

A section heading with automatic font-size scaling by level.

| Field | Default | Description |
|---|---|---|
| `text` | | The heading text. Supports tokens. |
| `level` | `1` | `1` = 20 pt, `2` = 16 pt, `3` = 13 pt. |
| `fontSize` | level default | Override the font size. |

```json
{ "type": "heading", "level": 2, "text": "Line Items" }
```

---

### `text`

A paragraph of styled text.

| Field | Default | Description |
|---|---|---|
| `text` | | The text content. Supports tokens. |
| `fontSize` | template default | Font size in pt. |
| `bold` | `false` | Semi-bold weight. |
| `italic` | `false` | Italic style. |
| `align` | left | `"left"`, `"center"`, `"right"`. |

```json
{ "type": "text", "text": "Invoice Date: {{invoice.date}}", "align": "right" }
```

---

### `spacer`

Vertical whitespace.

| Field | Default | Description |
|---|---|---|
| `height` | | Height in pt. |

```json
{ "type": "spacer", "height": 16 }
```

---

### `line`

A full-width horizontal rule (0.5 pt thick).

```json
{ "type": "line" }
```

---

### `pageBreak`

Forces a new page.

```json
{ "type": "pageBreak" }
```

---

### `watermark`

Renders large diagonal/centred text as the page background. Rendered behind all other content.

| Field | Default | Description |
|---|---|---|
| `text` | | Watermark text. Supports tokens (e.g. `{{document.watermark}}`). |
| `fontSize` | `64` | Font size in pt. |
| `color` | `"#E5E7EB"` | Hex colour. Use light values so body content remains readable. |

```json
{ "type": "watermark", "text": "DRAFT", "fontSize": 72, "color": "#EEEEEE" }
```

---

### `image`

Embeds a PNG or JPEG image. Source options (checked in order):

| Field | Description |
|---|---|
| `base64` | Base64-encoded image string, or a `{{token}}` that resolves to one. |
| `dataPath` | Dot-path into `data` whose string value is a base64 image. |
| `source` | Relative path inside the `/Assets/` directory. |

| Field | Default | Description |
|---|---|---|
| `width` | | Constrain to this width (pt). |
| `height` | | Constrain to this height (pt). When both width and height are set, `FitArea` is used. |
| `align` | left | `"left"`, `"center"`, `"right"`. |

> **SVG logos:** SVG files cannot be embedded directly. Convert first:
> ```bash
> rsvg-convert -w 400 logo.svg -o logo.png
> base64 -i logo.png | tr -d '\n'
> ```
> Paste the output as the `logoBase64` value in your request data.

```json
{
  "type": "image",
  "base64": "{{company.logoBase64}}",
  "width": 80,
  "height": 36,
  "align": "left"
}
```

---

### `twoColumn`

Lays out two independent content columns side by side. Child blocks within each column are stacked with no automatic spacing between them — add explicit `spacer` blocks where needed.

| Field | Default | Description |
|---|---|---|
| `leftBlocks` | `[]` | Array of block definitions for the left column. |
| `rightBlocks` | `[]` | Array of block definitions for the right column. |
| `leftWidth` | `1` | Relative width of the left column. |
| `rightWidth` | `1` | Relative width of the right column. `leftWidth: 2, rightWidth: 1` gives a 2:1 split. |
| `gap` | `20` | Fixed gap in pt between the two columns. |

```json
{
  "type": "twoColumn",
  "gap": 20,
  "leftWidth": 1,
  "rightWidth": 1,
  "leftBlocks": [
    { "type": "heading", "level": 1, "text": "Invoice", "fontSize": 22 }
  ],
  "rightBlocks": [
    { "type": "barcode", "value": "{{invoice.number}}", "width": 220, "height": 48, "align": "right" }
  ]
}
```

---

### `addressBlock`

A labelled address section (bold label + lines of smaller text).

| Field | Default | Description |
|---|---|---|
| `label` | | Bold label (e.g. company name or "Bill To"). Supports tokens. |
| `lines` | `[]` | Array of text strings. Supports tokens. Empty lines are skipped. |
| `fontSize` | `9` | Font size for the address lines. Label is always `10` pt. |
| `padding` | `0` | Padding around the whole block. |

```json
{
  "type": "addressBlock",
  "label": "{{company.name}}",
  "lines": [
    "{{company.abn}}",
    "{{company.email}}",
    "{{company.address.line1}}",
    "{{company.address.city}} {{company.address.state}} {{company.address.postcode}}"
  ]
}
```

---

### `certificateTitle`

A large centred title with an optional smaller subtitle. Always centre-aligned.

| Field | Default | Description |
|---|---|---|
| `text` | | Main title text. Supports tokens. |
| `caption` | | Subtitle text below the title. Supports tokens. |
| `fontSize` | `28` | Font size of the main title. Subtitle is fixed at `12` pt. |

```json
{
  "type": "certificateTitle",
  "text": "Certificate of Completion",
  "caption": "JSON-driven PDF Template API",
  "fontSize": 34
}
```

---

### `table`

A data table with a header row and configurable columns. Data can come from the `data` JSON (dynamic) or be defined inline (static).

**Column definition:**

| Field | Description |
|---|---|
| `header` | Column heading text. Right-aligned when `align` is set. |
| `value` | Token template for each cell (e.g. `{{description}}`). |
| `width` | Fixed column width in pt. Omit for proportional. |
| `align` | `"left"` (default), `"center"`, `"right"`. Applied to both header and cells. |

**Block fields:**

| Field | Default | Description |
|---|---|---|
| `columns` | **required** | Array of column definitions. |
| `showHeader` | `true` | Whether to render the header row. |
| `dataPath` | | Dot-path into `data` resolving to a JSON array. Each element is one row; column `value` tokens resolve per row. |
| `rows` | | Inline `string[][]` — static rows. Used when `dataPath` is absent. Tokens resolve against root `data`. |

```json
{
  "type": "table",
  "dataPath": "items",
  "showHeader": true,
  "columns": [
    { "header": "Description", "value": "{{description}}" },
    { "header": "Qty",        "value": "{{quantity}}",  "width": 50, "align": "right" },
    { "header": "Unit Price", "value": "{{unitPrice}}", "width": 80, "align": "right" },
    { "header": "Total",      "value": "{{total}}",     "width": 80, "align": "right" }
  ]
}
```

---

### `totalsBox`

A right-aligned bordered box of labelled amounts (subtotal, tax, total, etc.).

| Field | Default | Description |
|---|---|---|
| `keyValues` | **required** | Array of `{ label, value, bold?, fontSize? }`. Supports tokens. |
| `width` | `240` | Box width in pt. |
| `padding` | `8` | Inner padding in pt. |
| `align` | `"right"` | Box alignment (`"left"`, `"center"`, `"right"`). |

```json
{
  "type": "totalsBox",
  "align": "right",
  "width": 230,
  "padding": 10,
  "keyValues": [
    { "label": "Subtotal",   "value": "{{totals.subtotal}}" },
    { "label": "GST (10%)", "value": "{{totals.gst}}" },
    { "label": "Total Due",  "value": "{{totals.total}}", "bold": true, "fontSize": 13 }
  ]
}
```

---

### `approvalStamp`

A coloured bordered box displaying an approval status (APPROVED, DRAFT, PENDING, etc.). All text fields support tokens.

| Field | Default | Description |
|---|---|---|
| `status` | `"APPROVED"` | Large bold top line. Supports tokens — e.g. `"{{approval.status}}"`. |
| `value` | | Medium second line. E.g. `"Approved by {{approval.name}}"`. |
| `caption` | | Small third line. Typically a date. |
| `color` | `"#166534"` | Hex colour for the border and all text. Supports tokens — e.g. `"{{approval.color}}"`. If the token is absent from data, defaults to green. |
| `width` | `180` | Box width in pt. |
| `fontSize` | `16` | Font size of the status text. |
| `padding` | `8` | Inner padding in pt. |
| `align` | `"right"` | `"left"`, `"center"`, `"right"`. |

```json
{
  "type": "approvalStamp",
  "status": "{{approval.status}}",
  "value": "Approved by {{approval.name}}",
  "caption": "{{approval.date}}",
  "color": "{{approval.color}}",
  "align": "left",
  "width": 190
}
```

Data example with optional colour:
```json
"approval": {
  "status": "APPROVED",
  "name": "Finance",
  "date": "30 June 2026",
  "color": "#166534"
}
```
Omit `color` to use the default green.

---

### `signature`

A signature area with a blank space, a rule line, and label/name/caption.

| Field | Default | Description |
|---|---|---|
| `label` | `"Signature"` | Label under the rule (e.g. "Authorised Signatory"). |
| `value` | | Name printed below the label. |
| `caption` | | Small text below the name (e.g. `"Date: {{invoice.date}}"`). |
| `width` | `240` | Block width in pt. |
| `height` | `48` | Height of the blank signing space above the rule. |
| `fontSize` | `9` | Font size for label/name/caption. |
| `align` | left | `"left"`, `"center"`, `"right"`. |

```json
{
  "type": "signature",
  "label": "Customer Signature",
  "value": "{{customer.name}}",
  "caption": "Date: {{invoice.date}}",
  "width": 240,
  "height": 40,
  "align": "left"
}
```

---

### `qrCode`

A QR code image rendered at high pixel density (40 px per module) for crisp print output.

| Field | Default | Description |
|---|---|---|
| `value` | | URL or string to encode. Supports tokens. |
| `size` | `100` | Width and height in pt. |
| `caption` | | Small centred text below the QR code. |
| `align` | left | `"left"`, `"center"`, `"right"`. |

```json
{
  "type": "qrCode",
  "value": "{{invoice.paymentUrl}}",
  "size": 70,
  "caption": "Scan to pay",
  "align": "right"
}
```

---

### `barcode`

A CODE_128 barcode (or other ZXing-supported format).

| Field | Default | Description |
|---|---|---|
| `value` | | String to encode. Supports tokens. |
| `label` | `"CODE_128"` | Barcode format passed to ZXing. |
| `width` | `280` | Image width in pt. |
| `height` | `80` | Image height in pt. |
| `caption` | | Small centred text below the barcode. |
| `align` | left | `"left"`, `"center"`, `"right"`. |

```json
{
  "type": "barcode",
  "value": "{{invoice.number}}",
  "width": 220,
  "height": 48,
  "align": "right"
}
```

---

### `termsAndConditions`

A titled list of numbered lines, typically used for legal notices and payment terms.

| Field | Default | Description |
|---|---|---|
| `label` | `"Terms and Conditions"` | Bold section title (11 pt). |
| `text` | | Optional single paragraph below the title. |
| `lines` | `[]` | Array of strings. Each is auto-numbered. Supports tokens. |
| `fontSize` | `11` | Title font size. Lines are fixed at `8` pt. |

```json
{
  "type": "termsAndConditions",
  "label": "Payment Terms",
  "lines": [
    "Payment is due by {{invoice.dueDate}}.",
    "All prices are in AUD and include GST where applicable.",
    "Late payment may result in service suspension."
  ]
}
```

---

## Token Syntax

Tokens in text strings are replaced with values from the `data` object at render time. Unresolved tokens are left visible as `{{token}}` to make debugging easy.

| Syntax | Resolves to |
|---|---|
| `{{fieldName}}` | Top-level field (case-insensitive) |
| `{{parent.child}}` | Nested object property |
| `{{items.0.name}}` | Array index access |
| `{{$.path}}` or `{{$path}}` | `$` prefix is stripped, same resolution |

```json
{
  "data": {
    "invoice": { "number": "INV-1002", "date": "24 June 2026" },
    "items": [{ "description": "Licence", "total": "$499.00" }]
  }
}
```

```
{{invoice.number}}   → INV-1002
{{invoice.date}}     → 24 June 2026
{{items.0.total}}    → $499.00
```

---

## Request Formats

### Named template

Loads `/Templates/{name}.json`, applies optional patch, renders with your data:

```json
{
  "templateName": "invoice2",
  "templatePatch": { "margin": 36 },
  "data": { ... },
  "output": { "fileName": "my-invoice.pdf" }
}
```

### Inline template

The `template` object replaces the named file entirely:

```json
{
  "template": {
    "title": "",
    "pageSize": "A4",
    "orientation": "Portrait",
    "margin": 40,
    "defaultFontSize": 10,
    "blockSpacing": 6,
    "showPageNumbers": true,
    "blocks": [ ... ]
  },
  "data": { ... },
  "output": { "fileName": "custom.pdf" }
}
```

### Template patch

Apply partial overrides on top of a named or inline template. If `blocks` is supplied it **replaces** the entire block list.

```json
{
  "templateName": "invoice2",
  "templatePatch": {
    "margin": 32,
    "showPageNumbers": false
  },
  "data": { ... }
}
```

---

## Template Library

Pre-built templates live in `/Templates/`. Reference them by filename (without `.json`):

| Template | Description |
|---|---|
| `invoice` | Minimal single-column invoice — no company branding |
| `invoice2` | Full-featured invoice: logo, barcode, approval stamp, QR payment link |
| `quote` | Quote with approval stamp, totals box, acceptance signature |
| `receipt` | Payment receipt with verify QR |
| `certificate` | Landscape certificate of completion |
| `service-report` | Field service report with work/parts tables and dual signatures |
| `delivery-docket` | Delivery docket with tracking QR |

---

## Sample Requests

Ready-to-use request files live in `/requests/`. Each contains full data and can be sent as-is:

| File | Output |
|---|---|
| `sample-invoice.json` | Simple invoice |
| `sample-invoice2.json` | Full invoice with approval stamp |
| `sample-invoice3.json` | Invoice variant with real logo |
| `sample-quote.json` | Quote with orange pending stamp |
| `sample-receipt.json` | Payment receipt |
| `sample-certificate.json` | Landscape completion certificate |
| `sample-service-report.json` | Field service report |
| `sample-delivery-docket.json` | Delivery docket with real logo |
| `sample-certificate-classic.json` | Classic formal certificate (landscape) |
| `sample-certificate-modern-corporate.json` | Clean left-aligned corporate certificate |
| `sample-certificate-premium-gold.json` | Premium gold-accent certificate |
| `sample-certificate-compliance.json` | Audit-friendly compliance certificate (portrait) |
| `sample-certificate-achievement-badge.json` | Digital badge-style certificate |

---

## Adding a New Block Type

1. Create `Services/Blocks/MyBlockRenderer.cs` implementing `IBlockRenderer`.
2. Register it in `Program.cs`:
   ```csharp
   builder.Services.AddSingleton<IBlockRenderer, MyBlockRenderer>();
   ```
3. The `PdfRenderer` discovers it automatically via `IEnumerable<IBlockRenderer>`.

---

## CI/CD

GitHub Actions workflow at `.github/workflows/dotnet.yml` runs on every push to `main`:

1. Restore + build (Release)
2. Run `test-invoices.sh` — generates all sample PDFs
3. Upload all generated PDFs as build artifacts

---

## Development

```bash
# Build
dotnet build --configuration Release

# Run (http://localhost:5122)
dotnet run

# Generate all sample PDFs and test stamps
bash test-invoices.sh
```

**Runtime dependencies:** .NET 10 SDK. No external system libraries required — all font and image rendering is pure .NET.

**QuestPDF licence:** set to `LicenseType.Community` in `Program.cs`. Upgrade if your deployment exceeds Community limits.

---

## Licence

Apache 2.0 — see [LICENSE](LICENSE).
