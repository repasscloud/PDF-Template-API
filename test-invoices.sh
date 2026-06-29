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

echo "Generating invoice2-1003.pdf..."

curl --fail --silent --show-error \
  -X POST "$URL/pdf" \
  -H "Content-Type: application/json" \
  --data-binary "@requests/sample-invoice3.json" \
  --output "invoice2-1003.pdf"

echo "Created invoice2-1003.pdf"

# ==============================================================================
# Quote example
# Uses: watermark, twoColumn, image, addressBlock, certificateTitle, barcode,
# table, totalsBox, approvalStamp, termsAndConditions, signature.
# Output: quote-2001.pdf
# ==============================================================================

echo "Generating quote-2001.pdf..."

curl --fail --silent --show-error \
  -X POST "$URL/pdf" \
  -H "Content-Type: application/json" \
  --data-binary "@requests/sample-quote.json" \
  --output "quote-2001.pdf"

echo "Created quote-2001.pdf"


# ==============================================================================
# Payment receipt example
# Uses: watermark, twoColumn, image, addressBlock, certificateTitle,
# approvalStamp, qrCode, table, totalsBox, termsAndConditions.
# Output: receipt-3001.pdf
# ==============================================================================

echo "Generating receipt-3001.pdf..."

curl --fail --silent --show-error \
  -X POST "$URL/pdf" \
  -H "Content-Type: application/json" \
  --data-binary "@requests/sample-receipt.json" \
  --output "receipt-3001.pdf"

echo "Created receipt-3001.pdf"


# ==============================================================================
# Certificate example
# Uses: landscape page, watermark, image, certificateTitle, text alignment,
# twoColumn, signature, qrCode, approvalStamp.
# Output: certificate-4001.pdf
# ==============================================================================

echo "Generating certificate-4001.pdf..."

curl --fail --silent --show-error \
  -X POST "$URL/pdf" \
  -H "Content-Type: application/json" \
  --data-binary "@requests/sample-certificate.json" \
  --output "certificate-4001.pdf"

echo "Created certificate-4001.pdf"


# ==============================================================================
# Service report example
# Uses: watermark, twoColumn, image, addressBlock, certificateTitle,
# approvalStamp, barcode, tables, termsAndConditions, signatures.
# Output: service-report-5001.pdf
# ==============================================================================

echo "Generating service-report-5001.pdf..."

curl --fail --silent --show-error \
  -X POST "$URL/pdf" \
  -H "Content-Type: application/json" \
  --data-binary "@requests/sample-service-report.json" \
  --output "service-report-5001.pdf"

echo "Created service-report-5001.pdf"


# ==============================================================================
# Delivery docket example
# Uses: watermark, twoColumn, image, addressBlock, certificateTitle, barcode,
# qrCode, table, approvalStamp, signatures, termsAndConditions.
# Output: delivery-docket-6001.pdf
# ==============================================================================

echo "Generating delivery-docket-6001.pdf..."

curl --fail --silent --show-error \
  -X POST "$URL/pdf" \
  -H "Content-Type: application/json" \
  --data-binary "@requests/sample-delivery-docket.json" \
  --output "delivery-docket-6001.pdf"

echo "Created delivery-docket-6001.pdf"


# ==============================================================================
# Certificate of Completion — Classic Formal
# Uses: watermark, image, line, certificateTitle, text, signature, approvalStamp, qrCode
# Output: certificate-classic.pdf
# ==============================================================================

echo "Generating certificate-classic.pdf..."

curl --fail --silent --show-error \
  -X POST "$URL/pdf" \
  -H "Content-Type: application/json" \
  --data-binary "@requests/sample-certificate-classic.json" \
  --output "certificate-classic.pdf"

echo "Created certificate-classic.pdf"


# ==============================================================================
# Certificate of Completion — Modern Corporate
# Uses: image, heading, text, line, twoColumn, signature, qrCode
# Output: certificate-modern-corporate.pdf
# ==============================================================================

echo "Generating certificate-modern-corporate.pdf..."

curl --fail --silent --show-error \
  -X POST "$URL/pdf" \
  -H "Content-Type: application/json" \
  --data-binary "@requests/sample-certificate-modern-corporate.json" \
  --output "certificate-modern-corporate.pdf"

echo "Created certificate-modern-corporate.pdf"


# ==============================================================================
# Certificate of Completion — Premium Gold
# Uses: watermark, image, line, certificateTitle, text, signature, approvalStamp, qrCode
# Output: certificate-premium-gold.pdf
# ==============================================================================

echo "Generating certificate-premium-gold.pdf..."

curl --fail --silent --show-error \
  -X POST "$URL/pdf" \
  -H "Content-Type: application/json" \
  --data-binary "@requests/sample-certificate-premium-gold.json" \
  --output "certificate-premium-gold.pdf"

echo "Created certificate-premium-gold.pdf"


# ==============================================================================
# Certificate of Completion — Compliance / Audit Friendly (A4 Portrait)
# Uses: twoColumn, image, heading, line, text, termsAndConditions, signature, qrCode
# Output: certificate-compliance.pdf
# ==============================================================================

echo "Generating certificate-compliance.pdf..."

curl --fail --silent --show-error \
  -X POST "$URL/pdf" \
  -H "Content-Type: application/json" \
  --data-binary "@requests/sample-certificate-compliance.json" \
  --output "certificate-compliance.pdf"

echo "Created certificate-compliance.pdf"


# ==============================================================================
# Certificate of Completion — Achievement Badge
# Uses: twoColumn, image, heading, text, signature, approvalStamp, qrCode
# Output: certificate-achievement-badge.pdf
# ==============================================================================

echo "Generating certificate-achievement-badge.pdf..."

curl --fail --silent --show-error \
  -X POST "$URL/pdf" \
  -H "Content-Type: application/json" \
  --data-binary "@requests/sample-certificate-achievement-badge.json" \
  --output "certificate-achievement-badge.pdf"

echo "Created certificate-achievement-badge.pdf"


# ==============================================================================
# Stamp examples
#
# Two positioning modes are supported:
#
#   1. Named position  — set "position" to one of:
#        topRight (default), topLeft, bottomRight, bottomLeft, center
#      The stamp is placed 28 pt inside that corner automatically.
#
#   2. Explicit X/Y   — set "x" and "y" to exact PDF point coordinates.
#      Origin (0,0) is BOTTOM-LEFT. Y increases upward.
#      Common page sizes:
#        A4 Portrait  595 x 842 pt   (1 cm ≈ 28.35 pt)
#        A4 Landscape 842 x 595 pt
#
#      Formula for top-right of A4 portrait with a 125x84 pt stamp and 28 pt margin:
#        x = 595 - 28 - 125 = 442
#        y = 842 - 28 -  84 = 730
#
# ==============================================================================

# --- invoice2: named position (topRight) ---------------------------------------
echo "Stamping invoice2-1002.pdf -> invoice2-1002-approved.pdf (position: topRight)..."

INVOICE2_B64=$(base64 < "invoice2-1002.pdf" | tr -d '\n')

curl --fail --silent --show-error \
  -X POST "$URL/pdf/stamp" \
  -H "Content-Type: application/json" \
  -d "{
    \"pdfBase64\": \"$INVOICE2_B64\",
    \"status\": \"APPROVED\",
    \"value\": \"Approved by Finance\",
    \"caption\": \"$(date '+%d %B %Y')\",
    \"color\": \"#166534\",
    \"position\": \"topRight\",
    \"width\": 125,
    \"fileName\": \"invoice2-1002-approved.pdf\"
  }" \
  --output "invoice2-1002-approved.pdf"

echo "Created invoice2-1002-approved.pdf"

# --- invoice2: explicit X/Y — stamp shifted down by one stamp height ----------
# A4 portrait = 595 x 842 pt.  Stamp: 125 wide x ~84 tall.  28 pt margin.
# topRight baseline:  x = 595 - 28 - 125 = 442,  y = 842 - 28 - 84  = 730
# Shifted down 84 pt: x = 442,                    y = 730 - 84        = 646
echo "Stamping invoice2-1002.pdf -> invoice2-1002-approved-2.pdf (x/y: 442,646)..."

curl --fail --silent --show-error \
  -X POST "$URL/pdf/stamp" \
  -H "Content-Type: application/json" \
  -d "{
    \"pdfBase64\": \"$INVOICE2_B64\",
    \"status\": \"APPROVED\",
    \"value\": \"Approved by Finance\",
    \"caption\": \"$(date '+%d %B %Y')\",
    \"color\": \"#f12b8f\",
    \"x\": 442,
    \"y\": 646,
    \"width\": 125,
    \"fileName\": \"invoice2-1002-approved-2.pdf\"
  }" \
  --output "invoice2-1002-approved-2.pdf"

echo "Created invoice2-1002-approved-2.pdf"

# --- certificate: explicit X/Y coordinates ------------------------------------
# A4 landscape = 842 x 595 pt.  Stamp: 125 wide x ~84 tall.  28 pt margin.
# Top-right:  x = 842 - 28 - 125 = 689,  y = 595 - 28 - 84 = 483
echo "Stamping certificate-4001.pdf -> certificate-4001-verified.pdf (x/y: 689,483)..."

CERT_B64=$(base64 < "certificate-4001.pdf" | tr -d '\n')

curl --fail --silent --show-error \
  -X POST "$URL/pdf/stamp" \
  -H "Content-Type: application/json" \
  -d "{
    \"pdfBase64\": \"$CERT_B64\",
    \"status\": \"VERIFIED\",
    \"value\": \"Verified by Registrar\",
    \"caption\": \"$(date '+%d %B %Y')\",
    \"color\": \"#D4AF37\",
    \"x\": 689,
    \"y\": 483,
    \"width\": 125,
    \"fileName\": \"certificate-4001-verified.pdf\"
  }" \
  --output "certificate-4001-verified.pdf"

echo "Created certificate-4001-verified.pdf"

echo "Done."