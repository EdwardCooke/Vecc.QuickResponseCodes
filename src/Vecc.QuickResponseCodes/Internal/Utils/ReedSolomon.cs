using System;
using System.Threading;
using Vecc.QuickResponseCodes.Internal.Factories;

namespace Vecc.QuickResponseCodes.Internal.Utils
{
    public class ReedSolomon : IReedSolomon
    {
        private static readonly byte[][] _generatorPolynomials;
        private readonly IDataConverter _dataConverter;
        private readonly IGaloisField _galoisField;

        static ReedSolomon()
        {
            _generatorPolynomials = CreateDefaultGeneratorPolynomials();
        }

        public ReedSolomon(IDataConverter dataConverter, IGaloisFieldFactory galoisFieldFactory)
        {
            this._dataConverter = dataConverter;
            this._galoisField = galoisFieldFactory.Create(0x11D);
        }

        public byte[] GetErrorCorrectionBytes(byte[] input, int numErrorCorrectionBytes)
        {
            return this.GetErrorCorrectionBytes(new ArraySegment<byte>(input), numErrorCorrectionBytes);
        }

        private static byte[][] CreateDefaultGeneratorPolynomials()
        {
            var generatorPolynomials = new byte[128][];
            generatorPolynomials[1] = new byte[] {1, 1}; // g_1(x) = x + 1, which is the first-degree polynomial with root 2^0 in GF(2^8)
            return generatorPolynomials;
        }

        private byte[] GetGeneratorPolynomial(int degree)
        {
            // In a Reed-Solomon code, the generator polynomial of degree n is a polynomial
            // with roots 2^0, 2^1, ..., 2^(n-1). This is also expressed as:
            // g_n(x) = (x - 2^0) * (x - 2^1) * ... * (x - 2^(n-1)).
            var generatorPolynomial = Volatile.Read(ref _generatorPolynomials[degree]);
            if (generatorPolynomial == null)
            {
                var previousGeneratorPolynomial = this.GetGeneratorPolynomial(degree - 1); // g_{n-1}, which is a polynomial of degree n-1
                var multiplicand =
                    new byte[]
                    {
                        1, this._galoisField.Pow(2, degree - 1)
                    }; // x + 2^(n-1), which is the first-degree polynomial with root 2^(n-1) in GF(2^8)
                generatorPolynomial =
                    this._galoisField.MultiplyPolynomial(previousGeneratorPolynomial,
                                                         multiplicand); // g_n(x) = g_{n-1}(x) * (x + 2^(n-1)), which results in an n-degree polynomial
                Volatile.Write(ref _generatorPolynomials[degree],
                               generatorPolynomial); // might overwrite existing value, but individual elements are immutable, so whatever
            }
            return generatorPolynomial;
        }

        private byte[] GetErrorCorrectionBytes(ArraySegment<byte> input, int numErrorCorrectionBytes)
        {
            // In Reed-Solomon, the error correction bytes are the remainder once the input
            // (which has been multiplied by x^n) is divided by g_n(x) over GF(2^8).
            var shiftedInput = new byte[input.Count + numErrorCorrectionBytes];
            //var galoisField = this._galoisFieldFactory.Create(0x11D);
            this._dataConverter.CopyToArray(input, shiftedInput);
            var generatorPolynomial = this.GetGeneratorPolynomial(numErrorCorrectionBytes);
            return this._galoisField.PolynomialMod(shiftedInput, generatorPolynomial);
        }
    }
}
