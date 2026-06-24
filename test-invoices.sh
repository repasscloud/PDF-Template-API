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