using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vecc.QuickResponseCodes.Abstractions;
using Vecc.QuickResponseCodes.Api.Models;
using CodeImageFormat = Vecc.QuickResponseCodes.Api.Models.CodeImageFormat;
using ErrorToleranceLevel = Vecc.QuickResponseCodes.Api.Models.ErrorToleranceLevel;

namespace Vecc.QuickResponseCodes.Api.Controllers
{
    /// <summary>
    ///     Generate QR Codes
    /// </summary>
    [Route("v1")]
    [ApiExplorerSettings(GroupName = "v1")]
    public class QuickResponseCodeController : Controller
    {
        private readonly ILogger<QuickResponseCodeController> _logger;
        private readonly IQuickResponseCodeGenerator _quickResponseCodeGenerator;

        public QuickResponseCodeController(ILogger<QuickResponseCodeController> logger, IQuickResponseCodeGenerator quickResponseCodeGenerator)
        {
            this._logger = logger;
            this._quickResponseCodeGenerator = quickResponseCodeGenerator;
        }

        /// <summary>
        ///     Returns a QR code representing the specified request
        /// </summary>
        /// <param name="request">Data to use to generate the QR code and the resulting format</param>
        /// <returns>A response with the correct mime-type of the image and the binary data.</returns>
        [HttpPost]
        public async Task<IActionResult> GetQrCode([FromBody] GetQrCodeRequest request)
        {
            this._logger.LogInformation("Executing");
            var backgroundColor = request.BackgroundColor?.ToAbstractions() ?? Colors.Transparent;
            var border = request.Border ?? 4;
            var dimensions = request.Dimensions ?? 100;
            var foregroundColor = request.ForegroundColor?.ToAbstractions() ?? Colors.Black;
            var imageFormat = request.ImageFormat ?? CodeImageFormat.Png;

            try
            {
                var image = await this._quickResponseCodeGenerator.GetQuickResponseCodeAsync(request.Data,
                                                                                             (Abstractions.ErrorToleranceLevel)
                                                                                             (request.ErrorToleranceLevel ??
                                                                                              ErrorToleranceLevel.VeryLow),
                                                                                             (Abstractions.CodeImageFormat)imageFormat,
                                                                                             dimensions,
                                                                                             border,
                                                                                             backgroundColor,
                                                                                             foregroundColor);

                string mimeType;
                switch (imageFormat)
                {
                    case CodeImageFormat.Jpeg:
                        mimeType = "image/jpeg";
                        break;
                    case CodeImageFormat.Png:
                        mimeType = "image/png";
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(request.ImageFormat));
                }

                return this.File(image, mimeType);
            }
            catch (Exception e)
            {
                this._logger.LogError(new EventId(), e, "error");
                throw;
            }
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public Task<IActionResult> Test(string data,
                                        ErrorToleranceLevel errorToleranceLevel = ErrorToleranceLevel.VeryLow,
                                        CodeImageFormat imageFormat = CodeImageFormat.Jpeg)
        {
            return this.GetQrCode(new GetQrCodeRequest
                                  {
                                      Data = Encoding.ASCII.GetBytes(data),
                                      ErrorToleranceLevel = errorToleranceLevel
                                  });
        }
    }
}
