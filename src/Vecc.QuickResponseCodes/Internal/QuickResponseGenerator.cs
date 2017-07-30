using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vecc.QuickResponseCodes.Abstractions;
using Vecc.QuickResponseCodes.Internal.Factories;
using Vecc.QuickResponseCodes.Internal.Utils;

namespace Vecc.QuickResponseCodes.Internal
{
    public class QuickResponseGenerator : IQuickResponseCodeGenerator
    {
        private readonly IDataMatrixFactory _dataMatrixFactory;
        private readonly IErrorCorrection _errorCorrection;
        private readonly ISymbolTemplateFactory _symbolTemplateFactory;

        public QuickResponseGenerator(IDataMatrixFactory dataMatrixFactory, IErrorCorrection errorCorrection,
                                      ISymbolTemplateFactory symbolTemplateFactory)
        {
            this._dataMatrixFactory = dataMatrixFactory;
            this._errorCorrection = errorCorrection;
            this._symbolTemplateFactory = symbolTemplateFactory;
        }

        public Task<byte[]> GetQuickResponseCodeAsync(string data,
                                                      ErrorToleranceLevel errorToleranceLevel = ErrorToleranceLevel.VeryLow,
                                                      CodeImageFormat codeImageFormat = CodeImageFormat.Png,
                                                      int dimensions = 100,
                                                      int border = 4,
                                                      Color backgroundColor = null,
                                                      Color foregroundColor = null)
        {
            return this.GetQuickResponseCodeAsync(Encoding.Default.GetBytes(data), errorToleranceLevel, codeImageFormat, dimensions, border,
                                                  backgroundColor, foregroundColor);
        }

        public Task<byte[]> GetQuickResponseCodeAsync(byte[] data,
                                                      ErrorToleranceLevel errorToleranceLevel = ErrorToleranceLevel.VeryLow,
                                                      CodeImageFormat imageFormat = CodeImageFormat.Png,
                                                      int dimensions = 100,
                                                      int border = 4,
                                                      Color backgroundColor = null,
                                                      Color foregroundColor = null)
        {
            if (errorToleranceLevel < ErrorToleranceLevel.VeryLow || errorToleranceLevel > ErrorToleranceLevel.High)
            {
                throw new ArgumentOutOfRangeException(nameof(errorToleranceLevel));
            }

            if (imageFormat < CodeImageFormat.Jpeg || imageFormat > CodeImageFormat.Png)
            {
                throw new ArgumentOutOfRangeException(nameof(imageFormat));
            }

            if (backgroundColor == null)
            {
                backgroundColor = Colors.Transparent;
            }

            if (foregroundColor == null)
            {
                foregroundColor = Colors.Black;
            }

            this.ValidateInputData(data);

            var template = this.GetTemplate(data, errorToleranceLevel);
            var image = template.ToImage(dimensions, border, backgroundColor.ToColor(), foregroundColor.ToColor());

            using (var stream = new MemoryStream())
            {
                image.Save(stream, this.GetImageFormat(imageFormat));

                return Task.FromResult(stream.ToArray());
            }
        }

        private ISymbolTemplate GetTemplate(byte[] data, ErrorToleranceLevel errorToleranceLevel)
        {
            var requires16BitLength = false;
            var maxBytesInVersion9Code = this._errorCorrection.GetQuickResponseCodeVersionInfo(9).GetCorrectionInfo(errorToleranceLevel).TotalDataBytes;
            byte[] binaryData;

            if (maxBytesInVersion9Code - 2 < data.Length)
            {
                // This data requires a version 10 or higher code; will not fit in version 9 or lower.
                // Version 10 and higher codes require 16-bit data lengths.
                requires16BitLength = true;
            }

            using (var sh = this._dataMatrixFactory.Create())
            {
                sh.WriteNibble(0x04); // byte mode
                if (requires16BitLength)
                {
                    sh.WriteWord((ushort)data.Length);
                }
                else
                {
                    sh.WriteByte((byte)data.Length);
                }
                sh.WriteBytes(new ArraySegment<byte>(data));
                sh.WriteNibble(0x00); // terminator
                binaryData = sh.ToArray();
            }

            var finalMessageSequence =
                this._errorCorrection.GetMessageSequence(binaryData, errorToleranceLevel, out var qrCodeVersion, out var errorCorrectionLevel);
            var template = this._symbolTemplateFactory.Create(qrCodeVersion);

            template.ErrorToleranceLevel = errorCorrectionLevel;
            template.PopulateData(finalMessageSequence);
            template.Complete();
            return template;
        }

        private void ValidateInputData(byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (data.Any(inputChar => inputChar < 0x20 || inputChar > 0x7E))
            {
                throw new ArgumentException(
                                            "The input string is limited to the characters U+0020 .. U+007E (the 7-bit printable ISO-8859-1 characters).",
                                            nameof(data));
            }
        }

        private ImageFormat GetImageFormat(CodeImageFormat imageFormat)
        {
            switch (imageFormat)
            {
                case CodeImageFormat.Jpeg:
                    return ImageFormat.Jpeg;
                case CodeImageFormat.Png:
                    return ImageFormat.Png;
            }

            throw new ArgumentOutOfRangeException(nameof(imageFormat));
        }
    }
}
