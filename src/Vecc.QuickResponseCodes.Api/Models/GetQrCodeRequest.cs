namespace Vecc.QuickResponseCodes.Api.Models
{
    public class GetQrCodeRequest
    {
        public Color BackgroundColor { get; set; }
        public int? Border { get; set; }
        public byte[] Data { get; set; }
        public int? Dimensions { get; set; }
        public ErrorToleranceLevel? ErrorToleranceLevel { get; set; }
        public Color ForegroundColor { get; set; }
        public CodeImageFormat? ImageFormat { get; set; }
    }
}
