using System;
using System.Runtime.CompilerServices;

namespace Vecc.QuickResponseCodes.Internal.Utils
{
    // Represents a finite field GF(2^8) with a generator of 2.
    internal sealed class GaloisField : IGaloisField
    {
        private readonly int[] _discreteLogTable;
        private readonly uint[] _exponentiationTable;

        // Creates GF(2^8) from its reducer polynomial.
        // For example, the reducer polynomial x^8 + x^2 + 1
        // is represented by the input value 0x105 (binary 100000101).
        public GaloisField(uint reducerPolynomial)
        {
            // Prepare tables
            this._exponentiationTable = new uint[255]; // generators can only create 255 elements in GF(2^8)
            this._discreteLogTable = new int[256];

            // Fill in exponentiation and discrete log tables
            var exponentiationResult = this._exponentiationTable[0] = 1; // 2^0 = 1
            for (var i = 1; i < 255; i++)
            {
                exponentiationResult <<= 1; // multiplication by 2 is a simple bit shift
                if ((exponentiationResult & 0x100) != 0)
                {
                    exponentiationResult ^= reducerPolynomial; // modulus is subtraction, which in a GF is represented by XOR
                }
                //Debug.Assert(exponentiationResult <= 255, "Exponentiation result should fit within a byte.");
                this._exponentiationTable[i] = exponentiationResult;

                this._discreteLogTable[exponentiationResult] = i;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte Pow(byte a, int b)
        {
            if (a == 0 && b == 0)
            {
                throw new DivideByZeroException();
            }

            if (a == 0)
            {
                return 0;
            }
            if (b == 0)
            {
                return 1;
            }

            // a^b = (2^(log(a))^b = 2^(log(a) * b)
            return (byte)this._exponentiationTable[Mod255(this._discreteLogTable[a] * (long)b)];
        }

        public byte[] MultiplyPolynomial(byte[] a, byte[] b)
        {
            var newLen = checked(a.Length + b.Length - 1);
            var newPoly = new byte[newLen];

            for (var i = 0; i < a.Length; i++)
            {
                var valA = a[i];
                var powA = a.Length - i - 1;
                for (var j = 0; j < b.Length; j++)
                {
                    var valB = b[j];
                    var powB = b.Length - j - 1;
                    newPoly[newPoly.Length - (powA + powB) - 1] ^= this.Mul(valA, valB);
                }
            }

            return newPoly;
        }

        public byte[] PolynomialMod(byte[] dividend, byte[] divisor)
        {
            dividend = (byte[])dividend.Clone(); // mutating in place

            for (var i = 0; i + divisor.Length - 1 < dividend.Length; i++)
            {
                var multFactor = dividend[i];
                for (var j = 0; j < divisor.Length; j++)
                    dividend[i + j] ^= this.Mul(multFactor, divisor[j]);
            }

            var remainder = new byte[divisor.Length - 1];
            Buffer.BlockCopy(dividend, dividend.Length - remainder.Length, remainder, 0, remainder.Length);
            return remainder;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Mod255(int value)
        {
            var result = value % 255;
            return result >= 0 ? result : result + 255;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int Mod255(long value)
        {
            var result = value % 255;
            return (int)(result >= 0 ? result : result + 255);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte Mul(byte a, byte b)
        {
            if (a == 0 || b == 0)
            {
                return 0; // anything multiplied by 0 is 0
            }

            // a * b = 2^(log(a)) * 2^(log(b)) = 2^(log(a) + log(b))
            return (byte)this._exponentiationTable[Mod255(this._discreteLogTable[a] + this._discreteLogTable[b])];
        }
    }
}
