namespace Vecc.QuickResponseCodes.Internal.Utils
{
    public interface IGaloisField
    {
        byte[] MultiplyPolynomial(byte[] a, byte[] b);
        byte[] PolynomialMod(byte[] dividend, byte[] divisor);
        byte Pow(byte a, int b);
    }
}
