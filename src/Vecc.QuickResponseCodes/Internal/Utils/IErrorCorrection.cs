using Vecc.QuickResponseCodes.Abstractions;
using Vecc.QuickResponseCodes.Internal.Models;

namespace Vecc.QuickResponseCodes.Internal.Utils
{
    public interface IErrorCorrection
    {
        CodeVersion GetQuickResponseCodeVersionInfo(int version);

        ErrorCorrectionInfo GetErrorCorrectionInfo(int inputBytes,
                                                   ErrorToleranceLevel desiredLevel,
                                                   out int qrCodeVersion,
                                                   out ErrorToleranceLevel errorToleranceLevel);

        byte[] GetMessageSequence(byte[] input,
                                  ErrorToleranceLevel desiredLevel,
                                  out int qrCodeVersion,
                                  out ErrorToleranceLevel errorToleranceLevel);
    }
}
