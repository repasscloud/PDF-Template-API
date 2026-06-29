# Changelog

All notable changes to this project are documented here.

Versioning follows [Semantic Versioning](https://semver.org/).  
Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

---

## [1.3.0] — 2026-06-30

### Added
- **Explicit X/Y stamp positioning** — `POST /pdf/stamp` now accepts `x` and `y` (PDF points) for pixel-perfect stamp placement. Takes priority over the named `position` field.
- Full coordinate system documentation in `PdfStampRequest.cs` and README covering page sizes, origin, and position formulas.
- Test script demonstrates both modes: named position for invoice, explicit X/Y for certificate.

### Fixed
- **Stamps upside-down and 4× too small** — QuestPDF (via SkiaSharp) writes `[0.25 0 0 -0.25 0 pageHeight] cm` to the page CTM without wrapping it in `q/Q`, so it bled into every new content stream. `PdfStamper` now applies the exact inverse matrix `[4 0 0 -4 0 4×pageHeight]` at the start of each stamp stream, cancelling the scale and Y-flip. Stamp at `width: 125` now physically renders at 125 pt (≈ 4.4 cm) instead of 31 pt.

---

## [1.2.0] — 2026-06-30

### Added
- **`POST /pdf/stamp` endpoint** — overlays a coloured approval stamp on any existing PDF without regenerating the document. Powered by iText7.
- Named position support: `topRight` (default), `topLeft`, `bottomRight`, `bottomLeft`, `center`.
- Configurable `status`, `value` (secondary line), `caption` (date line), `color`, `width`, `allPages`, and `fileName`.
- `itext7` v7.2.6 added as a dependency.
- `PdfStampRequest` model with XML doc comments for all fields.
- `PdfStamper` service with automatic stamp height calculation, centred text rendering, and colour parsing.

### Changed
- Stamp font sizes increased: status 22 pt, value 11 pt, caption 10 pt — and inner padding 13 pt for a proper proportional box.
- Line spacing fixed: previously used `statusFontSize` as the inter-line step (wrong), now uses `lineGap + nextFontSize` so layout remains correct at all font sizes.

---

## [1.1.0] — 2026-06-30

### Added
- **Approval stamp colour tokens** — `color` field on `approvalStamp` blocks now resolves via the token system. Set `"color": "{{approval.color}}"` in the template and supply the hex value in request data. Falls back gracefully to default green if the token is absent.
- `invoice2` template wired to `{{approval.color}}`.
- `quote` template wired to `{{stamp.color}}` with the full `stamp` object in `sample-quote.json` (status, value, caption, color).
- **QR codes rendered at 2× pixel density** — `GetGraphic(20)` raised to `GetGraphic(40)` for crisp output at print resolution.

### Fixed
- `ApprovalStampBlockRenderer` now falls back to `#166534` when the resolved colour string does not start with `#` (catches unresolved `{{token}}` values).

---

## [1.0.0] — 2026-06-30

### Added
- **Five certificate of completion variants** (inline template format, no named template file needed):
  - Classic Formal (landscape, centred, navy stamp)
  - Modern Corporate (landscape, left-aligned, large QR)
  - Premium Gold (landscape, gold watermark and stamp)
  - Compliance / Audit Friendly (portrait, label/value layout)
  - Achievement Badge (landscape, twoColumn with blue badge)
- All five added to `test-invoices.sh`.
- `dataPath` image resolution in `ImageSourceResolver` — image `base64` field now resolves tokens, enabling `"dataPath": "logoBase64"` for cleaner inline templates.
- `heading` block `fontSize` override support.
- Table column header alignment — `align` field now applies to header cells as well as body cells (`TableBlockRenderer` fix).

### Changed
- **Unified two-section header layout** across all document templates:
  - Section 1 twoColumn: document title + reference number (left), barcode + dates (right). Starts at the top of the page with no gap.
  - Section 2 twoColumn: company logo + sender address (left), matching spacer + recipient address (right). Company name and recipient label land at identical height.
- **`approvalStamp` LEFT of totals box** on invoice2 and quote — stamp and totals are in separate twoColumn columns.
- Removed redundant `title` page headers from all templates — document type and reference already shown in the body.
- Removed redundant "Invoice Number / Receipt Number / ..." text blocks (shown in barcode caption and header).
- Certificate template:
  - VERIFIED stamp removed from body — replaced with a single centred line showing certificate number and issue date.
  - Large spacers added (50 pt each) before and after recipient text so content fills the landscape page evenly.
  - Margin reduced from 50 to 40 pt.
- Delivery docket: barcode caption removed (number already prominent on the left).
- All portrait templates: `blockSpacing` reduced from 8 to 6 pt for tighter, more professional spacing.
- Approval stamp and QR code for certificates placed in separate twoColumn rows to prevent overlap.
- `sample-invoice2.json` and `sample-quote.json`: removed `templatePatch.title` (no longer needed).
- Logo image height set to `20 pt` across all portrait templates with matching right-column spacer (`24 pt`) to keep address blocks vertically aligned.

### Removed
- VERIFIED / COMPLETED / PAID stamps from receipt, service-report, and certificate (stamps now applied post-generation via `/pdf/stamp` instead).

---

## [0.3.0] — 2026-06-24

### Added
- **Extended document template library:** quote, payment receipt, certificate of completion (landscape), service report, delivery docket.
- `test-invoices.sh` extended to generate all document types end-to-end.
- All new templates use: `watermark`, `twoColumn`, `image`, `addressBlock`, `certificateTitle`, `barcode`, `qrCode`, `table`, `totalsBox`, `approvalStamp`, `signature`, `termsAndConditions`.
- 1×1 pixel transparent PNG placeholder for logo fields so examples run without asset files.

---

## [0.2.0] — 2026-06-24

### Added
- GitHub Actions CI workflow (`.github/workflows/dotnet.yml`): restore → build (Release) → run test script → upload generated PDFs as artifacts.
- `actions/upload-artifact@v7` pinned.
- Apache 2.0 licence.
- `.gitignore` updated for .NET and macOS artefacts.

---

## [0.1.0] — 2026-06-24

### Added
- Initial ASP.NET Core 10 Web API.
- `GET /` — health check returning service info.
- `POST /pdf` — generates a PDF from a JSON template definition + data object.
- **17 block renderers:** `heading`, `text`, `spacer`, `line`, `pageBreak`, `table`, `signature`, `qrCode`, `image`, `twoColumn`, `addressBlock`, `totalsBox`, `watermark`, `termsAndConditions`, `barcode`, `certificateTitle`, `approvalStamp`.
- `{{token.path}}` resolution with dot-path navigation, array index access, and case-insensitive property matching.
- `TemplateMerger` — shallow-merges a `PdfTemplatePatch` onto a loaded or inline template.
- `TemplateStore` — loads `/Templates/*.json`, validates block count and types.
- `TokenResolver` — resolves `{{path}}`, `{{$.path}}`, `{{$path}}` from any `JsonElement`.
- `ImageSourceResolver` — resolves images from base64 string, data path, or `/Assets/` file.
- `BarcodeImageGenerator` — ZXing.Net CODE_128 renderer.
- `invoice.json` and `invoice2.json` templates with sample request files.
- `test-invoices.sh` — end-to-end bash script: starts API, generates PDFs, asserts success, shuts down.
- QuestPDF Community licence.

[1.3.0]: https://github.com/repasscloud/PDF-Template-API/compare/v1.2.0...v1.3.0
[1.2.0]: https://github.com/repasscloud/PDF-Template-API/compare/v1.1.0...v1.2.0
[1.1.0]: https://github.com/repasscloud/PDF-Template-API/compare/v1.0.0...v1.1.0
[1.0.0]: https://github.com/repasscloud/PDF-Template-API/compare/v0.3.0...v1.0.0
[0.3.0]: https://github.com/repasscloud/PDF-Template-API/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/repasscloud/PDF-Template-API/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/repasscloud/PDF-Template-API/releases/tag/v0.1.0
