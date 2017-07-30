using System;
using System.IO;

namespace Vecc.QuickResponseCodes.Internal.Utils
{
    public class DataMatrix : IDataMatrix
    {
        private readonly MemoryStream _buffer = new MemoryStream();
        private int _bufferBits;
        private int _bufferBitsCount;

        public void WriteByte(byte b)
        {
            this.WriteBits(b, 8);
        }

        public void WriteBytes(ArraySegment<byte> bytes)
        {
            foreach (var b in bytes)
            {
                this.WriteByte(b);
            }
        }

        public void WriteNibble(byte value)
        {
            this.WriteBits(value, 4);
        }

        public void WriteWord(ushort value)
        {
            this.WriteBits(value, 16);
        }

        public byte[] ToArray()
        {
            return this._buffer.ToArray();
        }

        public void Dispose()
        {
            //clean up after ourselves.
            this._buffer?.Dispose();
        }

        private void WriteBits(int value, int bits)
        {
            if (bits > 24)
            {
                this.WriteBitsImpl(value >> 24, bits - 24);
            }
            if (bits > 16)
            {
                this.WriteBitsImpl(value >> 16, Math.Min(8, bits - 16));
            }
            if (bits > 8)
            {
                this.WriteBitsImpl(value >> 8, Math.Min(8, bits - 8));
            }
            this.WriteBitsImpl(value, Math.Min(8, bits));
        }

        private void WriteBitsImpl(int value, int bits)
        {
            if (bits == 0)
            {
                return;
            }

            var maskedValue = value & (0xFF >> (8 - bits));
            var newBufferBits = (this._bufferBits << bits) | maskedValue;
            var newBufferBitsCount = this._bufferBitsCount + bits;

            if (newBufferBitsCount >= 8)
            {
                var remainingBits = newBufferBitsCount - 8;

                this._buffer.WriteByte((byte)(newBufferBits >> remainingBits));
                this._bufferBits = newBufferBits & ((1 << remainingBits) - 1);
                this._bufferBitsCount = remainingBits;
            }
            else
            {
                this._bufferBits = newBufferBits;
                this._bufferBitsCount = newBufferBitsCount;
            }
        }
    }
}
