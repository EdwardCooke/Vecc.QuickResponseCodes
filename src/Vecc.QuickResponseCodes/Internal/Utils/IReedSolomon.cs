namespace Vecc.QuickResponseCodes.Internal.Utils
{
    public interface IReedSolomon
    {
        byte[] GetErrorCorrectionBytes(byte[] input, int numErrorCorrectionBytes);
    }
}
