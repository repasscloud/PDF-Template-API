# Additional PDF Template Examples

Copy the files from `Templates/` into your app's `Templates/` directory.

Copy the files from `requests/` into your app's `requests/` directory.

Append the contents of `test-invoices-additions.sh` to your existing `test-invoices.sh` after the API readiness check.

Examples included:

- `quote.json` + `sample-quote.json`
- `receipt.json` + `sample-receipt.json`
- `certificate.json` + `sample-certificate.json`
- `service-report.json` + `sample-service-report.json`
- `delivery-docket.json` + `sample-delivery-docket.json`

These are designed to use only the controlled block renderers already discussed:

- `watermark`
- `twoColumn`
- `image`
- `addressBlock`
- `certificateTitle`
- `barcode`
- `qrCode`
- `table`
- `totalsBox`
- `approvalStamp`
- `signature`
- `termsAndConditions`
- `text`
- `spacer`
- `line`

The image blocks use a 1x1 PNG base64 placeholder so the examples run without requiring asset files.
