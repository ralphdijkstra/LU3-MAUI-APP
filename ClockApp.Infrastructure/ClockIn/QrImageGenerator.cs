using QRCoder;

namespace ClockApp.Infrastructure.ClockIn;

public sealed class QrImageGenerator
{
    public byte[] CreatePng(string code)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(code, QRCodeGenerator.ECCLevel.Q);
        var pngQr = new PngByteQRCode(data);

        return pngQr.GetGraphic(8);
    }
}
