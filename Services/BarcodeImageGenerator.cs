using ZXing;
using ZXing.Common;

namespace PdfTemplateApi.Services;

public sealed class BarcodeImageGenerator
{
    public byte[] Generate(
        string value,
        string? format,
        int width,
        int height)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException("Barcode value is required.");

        var barcodeFormat = ResolveBarcodeFormat(format);

        var matrix = new MultiFormatWriter().encode(
            value,
            barcodeFormat,
            width,
            height,
            new Dictionary<EncodeHintType, object>
            {
                [EncodeHintType.MARGIN] = 2
            });

        var rgba = ToRgba(matrix);

        return PngWriter.WriteRgba(
            rgba,
            matrix.Width,
            matrix.Height);
    }

    private static byte[] ToRgba(BitMatrix matrix)
    {
        var rgba = new byte[matrix.Width * matrix.Height * 4];

        for (var y = 0; y < matrix.Height; y++)
        {
            for (var x = 0; x < matrix.Width; x++)
            {
                var offset = ((y * matrix.Width) + x) * 4;

                var isBlack = matrix[x, y];

                var value = isBlack
                    ? (byte)0
                    : (byte)255;

                rgba[offset + 0] = value; // R
                rgba[offset + 1] = value; // G
                rgba[offset + 2] = value; // B
                rgba[offset + 3] = 255;   // A
            }
        }

        return rgba;
    }

    private static BarcodeFormat ResolveBarcodeFormat(string? format)
    {
        return format?
            .Replace("-", "", StringComparison.Ordinal)
            .Replace("_", "", StringComparison.Ordinal)
            .ToUpperInvariant() switch
        {
            "CODE39" => BarcodeFormat.CODE_39,
            "CODE93" => BarcodeFormat.CODE_93,
            "CODE128" => BarcodeFormat.CODE_128,
            "EAN8" => BarcodeFormat.EAN_8,
            "EAN13" => BarcodeFormat.EAN_13,
            "UPCA" => BarcodeFormat.UPC_A,
            "UPCE" => BarcodeFormat.UPC_E,
            "PDF417" => BarcodeFormat.PDF_417,
            "DATAMATRIX" => BarcodeFormat.DATA_MATRIX,
            "AZTEC" => BarcodeFormat.AZTEC,
            "QR" => BarcodeFormat.QR_CODE,
            "QRCODE" => BarcodeFormat.QR_CODE,
            _ => BarcodeFormat.CODE_128
        };
    }
}