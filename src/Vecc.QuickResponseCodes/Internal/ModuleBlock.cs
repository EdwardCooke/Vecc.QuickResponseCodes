using System;
using System.Diagnostics;
using Vecc.QuickResponseCodes.Internal.Enums;

namespace Vecc.QuickResponseCodes.Internal
{
    internal sealed class ModuleBlock
    {
        private readonly byte[] _buffer;
        private readonly int _width;

        public ModuleBlock(int width)
        {
            Debug.Assert(0 <= width);
            this._buffer = new byte[checked((width * width + 3) / 4)]; // CEIL(width * width / 4)
            this._width = width;
        }

        private ModuleBlock(ModuleBlock original)
        {
            this._buffer = new byte[original._buffer.Length];
            Buffer.BlockCopy(original._buffer, 0, this._buffer, 0, this._buffer.Length);
            this._width = original._width;
        }

        private void CheckParameters(int row, int col)
        {
            //if (row >= 0 && row < this._width)
            //{
            //    throw new ArgumentOutOfRangeException(nameof(row));
            //}
            //if (col >= 0 && col < this._width)
            //{
            //    throw new ArgumentOutOfRangeException(nameof(col));
            //}
        }

        internal ModuleBlock CreateCopy()
        {
            return new ModuleBlock(this);
        }

        public void FlipColor(int row, int col)
        {
            this.CheckParameters(row, col);

            this.GetOffsets(row, col, out var blockNum, out var blockOffset);
            this._buffer[blockNum] ^= (byte)(0x1 << (2 * blockOffset));
        }

        public bool IsDark(int row, int col)
        {
            this.CheckParameters(row, col);
            return (this.GetFlag(row, col) & 1) == 1;
        }

        public bool IsLight(int row, int col)
        {
            return !this.IsDark(row, col);
        }

        public bool IsReserved(int row, int col)
        {
            this.CheckParameters(row, col);
            return (this.GetFlag(row, col) & 2) == 2;
        }

        public void Set(int row, int col, ModuleFlag flag, bool overwrite = false)
        {
            this.CheckParameters(row, col);
            this.SetFlag(row, col, flag, overwrite);
        }

        private int GetFlag(int row, int col)
        {
            this.GetOffsets(row, col, out var blockNum, out var blockOffset);
            return this.CalculateFlag(blockNum, blockOffset);
        }

        private void SetFlag(int row, int col, ModuleFlag flag, bool overwrite)
        {
            this.GetOffsets(row, col, out var blockNum, out var blockOffset);
            var leftShiftValue = 2 * blockOffset;
            var newFlag = ((int)flag & 0x3) << leftShiftValue;

            if (overwrite)
            {
                var originalValue = this._buffer[blockNum];
                var originalValueMasked = originalValue & ~(0x3 << leftShiftValue);
                var newValue = originalValueMasked | newFlag;
                this._buffer[blockNum] = (byte)newValue;
            }
            else
            {
                this._buffer[blockNum] |= (byte)newFlag;
            }
        }

        private void GetOffsets(int row, int col, out int blockNum, out int blockOffset)
        {
            var elementPos = row * this._width + col;
            blockNum = elementPos / 4;
            blockOffset = elementPos % 4;
        }

        private int CalculateFlag(int blockNum, int blockOffset)
        {
            return (this._buffer[blockNum] >> (2 * blockOffset)) & 0x03;
        }
    }
}
