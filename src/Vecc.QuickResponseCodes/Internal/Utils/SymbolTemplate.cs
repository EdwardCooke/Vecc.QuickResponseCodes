using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Vecc.QuickResponseCodes.Abstractions;
using Vecc.QuickResponseCodes.Internal.Enums;
using Color = System.Drawing.Color;

namespace Vecc.QuickResponseCodes.Internal.Utils
{
    public partial class SymbolTemplate : ISymbolTemplate
    {
        private static readonly int[] _formatInformationEncodings = GetFormatInformationEncodings();
        private static readonly int[] _versionInformationEncodings = GetVersionInformationEncodings();
        private readonly IAlignmentPattern _alignmentPattern;
        private readonly IMapping _mapping;
        private readonly int _version;
        private ModuleBlock _modules;

        public SymbolTemplate(IAlignmentPattern alignmentPattern, IMapping mapping)
        {
            this._alignmentPattern = alignmentPattern;
            this._mapping = mapping;
        }

        public SymbolTemplate(IAlignmentPattern alignmentPattern, IMapping mapping, int version)
            : this(alignmentPattern, mapping)
        {
            this._version = version;

            this.Width = 21 + 4 * (version - 1);
            this._modules = new ModuleBlock(this.Width);
            this.PopulateReservedModuleBits();
        }

        internal SymbolTemplate(SymbolTemplate prototype)
            : this(prototype._alignmentPattern, prototype._mapping)
        {
            this._modules = prototype._modules.CreateCopy();
            this.ErrorToleranceLevel = prototype.ErrorToleranceLevel;
            this._version = prototype._version;
            this.Width = prototype.Width;
        }

        public int Width { get; }

        public ErrorToleranceLevel ErrorToleranceLevel { get; set; }

        public void Complete()
        {
            var maskingPattern = this.SelectAndApplyMaskingPattern();
            var formatInformationDataBits = (GetBinaryIndicatorForErrorCorrectionLevel(this.ErrorToleranceLevel) << 3) | maskingPattern;
            var formatInformationEncodedBits = _formatInformationEncodings[formatInformationDataBits];
            this.ApplyFormatInformationEncodedBits(formatInformationEncodedBits);
        }

        public Image ToImage(int imageWidth, int borderWidth, Color background, Color foreground)
        {
            if (borderWidth < 4)
            {
                borderWidth = 4;
            }

            var totalModuleWidth = this.Width + borderWidth * 2;

            var moduleSize = imageWidth / totalModuleWidth;
            var borderSize = borderWidth * moduleSize;
            var imageDimensions = totalModuleWidth * moduleSize;
            var innerBorder = imageDimensions - borderSize;

            var pixels = new Color[imageDimensions * imageDimensions];

            this.FillRectangle(pixels, imageDimensions, 0, 0, imageDimensions, borderSize, background); //top
            this.FillRectangle(pixels, imageDimensions, 0, borderSize, borderSize, innerBorder, background); //left
            this.FillRectangle(pixels, imageDimensions, innerBorder, borderSize, borderSize, innerBorder, background); //right
            this.FillRectangle(pixels, imageDimensions, borderSize, innerBorder, innerBorder - borderSize, borderSize, background); //bottom

            for (var y = 0; y < this.Width; y++)
            for (var x = 0; x < this.Width; x++)
            {
                var color = background;
                if (this._modules.IsDark(y, x))
                {
                    color = foreground;
                }
                this.FillRectangle(pixels, imageDimensions, x * moduleSize + borderSize, y * moduleSize + borderSize, moduleSize, moduleSize, color);
            }

            using (var image = new Bitmap(imageDimensions, imageDimensions, PixelFormat.Format32bppArgb))
            {
                this.DrawImage(image, pixels);

                var finalImage = new Bitmap(image, new Size(imageWidth, imageWidth));
                return finalImage;
            }
        }

        public bool IsReservedModule(int row, int col)
        {
            return this._modules.IsReserved(row, col);
        }

        public void PopulateData(byte[] rawData)
        {
            using (var ms = new MemoryStream(rawData))
            {
                foreach (var placement in this._mapping.GetCodewordPlacements(this))
                {
                    var thisByte = ms.ReadByte();
                    if (thisByte > byte.MaxValue)
                    {
                        return;
                    }
                    this._modules.Set(placement.Bit7.Y, placement.Bit7.X, GetTypeFromBitPosition(thisByte, 7));
                    this._modules.Set(placement.Bit6.Y, placement.Bit6.X, GetTypeFromBitPosition(thisByte, 6));
                    this._modules.Set(placement.Bit5.Y, placement.Bit5.X, GetTypeFromBitPosition(thisByte, 5));
                    this._modules.Set(placement.Bit4.Y, placement.Bit4.X, GetTypeFromBitPosition(thisByte, 4));
                    this._modules.Set(placement.Bit3.Y, placement.Bit3.X, GetTypeFromBitPosition(thisByte, 3));
                    this._modules.Set(placement.Bit2.Y, placement.Bit2.X, GetTypeFromBitPosition(thisByte, 2));
                    this._modules.Set(placement.Bit1.Y, placement.Bit1.X, GetTypeFromBitPosition(thisByte, 1));
                    this._modules.Set(placement.Bit0.Y, placement.Bit0.X, GetTypeFromBitPosition(thisByte, 0));

                    if (ms.Position == ms.Length)
                    {
                        break;
                    }
                }
                ms.ReadByte();
            }
        }

        private void DrawImage(Bitmap image, Color[] pixels)
        {
            var bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            var scan0 = bitmapData.Scan0;
            var rawDataLength = Math.Abs(bitmapData.Stride) * image.Height;
            var rawData = new byte[rawDataLength];
            Marshal.Copy(scan0, rawData, 0, rawDataLength);

            for (var pixelIndex = 0; pixelIndex < pixels.Length; pixelIndex++)
            {
                var pixel = pixels[pixelIndex];

                rawData[pixelIndex * 4] = pixel.B; //BLUE
                rawData[pixelIndex * 4 + 1] = pixel.G; //GREEN
                rawData[pixelIndex * 4 + 2] = pixel.R; //RED
                rawData[pixelIndex * 4 + 3] = pixel.A; //ALPHA
            }

            Marshal.Copy(rawData, 0, scan0, rawDataLength);
            image.UnlockBits(bitmapData);
        }

        private void PopulateReservedModuleBits()
        {
            // This isn't the most efficient way to fill in the template, but it's only
            // performed once, then cached. So speed isn't a huge issue.

            // Fill in timing pattern in row 6 and col 6.
            // ISO/IEC 18004:2006(E), Sec. 5.3.4
            for (var i = 0; i < this.Width; i++)
            {
                var moduleFlag = ModuleFlag.Reserved | (i % 2 == 0 ? ModuleFlag.Dark : ModuleFlag.Light);
                this._modules.Set(i, 6, moduleFlag, true);
                this._modules.Set(6, i, moduleFlag, true);
            }

            // Fill in the finder patterns (the "QR" patterns)
            this.PopulateFinderPatterns();

            // Fill in alignment patterns (the smaller "QR" patterns throughout the encoding region)
            this.PopulateAlignmentPatterns();

            // Fill in version information (next to the finder pattern)
            this.PopulateVersionInformation();
        }

        private void PopulateFinderPatterns()
        {
            // Finder pattern is a 3x3 dark square inscribed within a 5x5 light square inscribed within a 7x7 dark square.
            // The finder pattern is separated from the rest of the data section by a one-module width line.
            // See ISO/IEC 18004:2006(E), Sec. 5.3.2.1 and Sec. 5.3.3

            // Top-left
            this.DrawSquare(this._modules, 0, 0, 9, ModuleFlag.Reserved, true); // reserved for format information
            this.DrawSquare(this._modules, 0, 0, 8, ModuleFlag.Reserved | ModuleFlag.Light); // separator
            this.DrawSquare(this._modules, 0, 0, 7, ModuleFlag.Reserved | ModuleFlag.Dark);
            this.DrawSquare(this._modules, 1, 1, 5, ModuleFlag.Reserved | ModuleFlag.Light);
            this.DrawSquare(this._modules, 2, 2, 3, ModuleFlag.Reserved | ModuleFlag.Dark);

            // Top-right
            this.DrawRect(this._modules, 8, this.Width - 8, 8, 1, ModuleFlag.Reserved, true); // reserved for format information
            this.DrawSquare(this._modules, 0, this.Width - 8, 8, ModuleFlag.Reserved | ModuleFlag.Light); // separator
            this.DrawSquare(this._modules, 0, this.Width - 7, 7, ModuleFlag.Reserved | ModuleFlag.Dark);
            this.DrawSquare(this._modules, 1, this.Width - 6, 5, ModuleFlag.Reserved | ModuleFlag.Light);
            this.DrawSquare(this._modules, 2, this.Width - 5, 3, ModuleFlag.Reserved | ModuleFlag.Dark);

            // Bottom-left
            this.DrawRect(this._modules, this.Width - 8, 8, 1, 8, ModuleFlag.Reserved, true); // reserved for format information
            this.DrawSquare(this._modules, this.Width - 8, 0, 8, ModuleFlag.Reserved | ModuleFlag.Light); // separator
            this.DrawSquare(this._modules, this.Width - 7, 0, 7, ModuleFlag.Reserved | ModuleFlag.Dark);
            this.DrawSquare(this._modules, this.Width - 6, 1, 5, ModuleFlag.Reserved | ModuleFlag.Light);
            this.DrawSquare(this._modules, this.Width - 5, 2, 3, ModuleFlag.Reserved | ModuleFlag.Dark);
        }

        private void PopulateAlignmentPatterns()
        {
            // Alignment pattern is a 1x1 dark square inscribed within a 3x3 light square inscribed within a 5x5 dark square.
            // See ISO/IEC 18004:2006(E), Sec. 5.3.5
            foreach (var coord in this._alignmentPattern.GetCoordinates(this._version))
            {
                this.DrawSquare(this._modules, coord.Y - 2, coord.X - 2, 5, ModuleFlag.Reserved | ModuleFlag.Dark);
                this.DrawSquare(this._modules, coord.Y - 1, coord.X - 1, 3, ModuleFlag.Reserved | ModuleFlag.Light);
                this.DrawSquare(this._modules, coord.Y, coord.X, 1, ModuleFlag.Reserved | ModuleFlag.Dark);
            }
        }

        private void PopulateVersionInformation()
        {
            // Version information appears next to the QR finder pattern
            // See ISO/IEC 18004:2006(E), Sec. 6.10

            // Pattern only appears in QR code version 7 and greater
            if (this._version < 7)
            {
                return;
            }

            // The version information is an 18-bit sequence: the 6-bit version number and a 12-bit error correction code
            var versionInformation = _versionInformationEncodings[this._version - 7];

            for (var i = 0; i < 18; i++)
            {
                var type = ModuleFlag.Reserved | (((versionInformation >> i) & 0x2) != 0 ? ModuleFlag.Dark : ModuleFlag.Light);
                var x = i / 3;
                var y = this.Width - 11 + i % 3;
                this._modules.Set(x, y, type, true); // upper-right
                this._modules.Set(y, x, type, true); // lower-left
            }
        }

        private void DrawSquare(ModuleBlock modules, int row, int col, int width, ModuleFlag type, bool xor = false)
        {
            this.DrawRect(modules, row, col, width, width, type, xor);
        }

        private void DrawRect(ModuleBlock modules, int row, int col, int width, int height, ModuleFlag type, bool xor = false)
        {
            xor = !xor;
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
                modules.Set(row + y, col + x, type, xor);
        }

        private static int[] GetVersionInformationEncodings()
        {
            // Version information encodings are given in ISO/IEC 18004:2006(E), Annex D, Table D.1
            return new[]
                   {
                       0x07C94, // version 7
                       0x085BC, // version 8
                       0x09A99, // ...
                       0x0A4D3,
                       0x0BBF6,
                       0x0C762,
                       0x0D847,
                       0x0E60D,
                       0x0F928,
                       0x10B78,
                       0x1145D,
                       0x12A17,
                       0x13532,
                       0x149A6,
                       0x15683,
                       0x168C9,
                       0x177EC,
                       0x18EC4,
                       0x191E1,
                       0x1AFAB,
                       0x1B08E,
                       0x1CC1A,
                       0x1D33F,
                       0x1ED75,
                       0x1F250,
                       0x209D5,
                       0x216F0,
                       0x228BA,
                       0x2379F,
                       0x24B0B,
                       0x2542E,
                       0x26A64,
                       0x27541,
                       0x28C69 // version 40
                   };
        }

        private static int[] GetFormatInformationEncodings()
        {
            // Format information encodings are given in ISO/IEC 18004:2006(E), Annex C, Table C.1
            return new[]
                   {
                       0x5412, // data = 00000
                       0x5125, // data = 00001
                       0x5E7C, // data = 00010
                       0x5B4B, // ...
                       0x45F9,
                       0x40CE,
                       0x4F97,
                       0x4AA0,
                       0x77C4,
                       0x72F3,
                       0x7DAA,
                       0x789D,
                       0x662F,
                       0x6318,
                       0x6C41,
                       0x6976,
                       0x1689,
                       0x13BE,
                       0x1CE7,
                       0x19D0,
                       0x0762,
                       0x0255,
                       0x0D0C,
                       0x083B,
                       0x355F,
                       0x3068,
                       0x3F31,
                       0x3A06,
                       0x24B4,
                       0x2183,
                       0x2EDA,
                       0x2BED // data = 11111
                   };
        }

        private void FillRectangle(Color[] lockedBits, int imageWidth, int x, int y, int width, int height, Color color)
        {
            //var startByte = x * 3;
            //var endByte = width * 3 + startByte;
            var maxY = y + height;

            for (var yPos = y; yPos < maxY; yPos++)
            {
                var startIndex = imageWidth * yPos + x;
                var endIndex = startIndex + width;

                for (var xPos = startIndex; xPos < endIndex; xPos++)
                    lockedBits[xPos] = color;
            }
        }

        private void ApplyFormatInformationEncodedBits(int bits)
        {
            // See ISO/IEC 18004:2006(E), Sec. 6.9.1, Figure 25

            var b14 = GetTypeFromBitPosition(bits, 14);
            this._modules.Set(8, 0, b14);
            this._modules.Set(this.Width - 1, 8, b14);

            var b13 = GetTypeFromBitPosition(bits, 13);
            this._modules.Set(8, 1, b13);
            this._modules.Set(this.Width - 2, 8, b13);

            var b12 = GetTypeFromBitPosition(bits, 12);
            this._modules.Set(8, 2, b12);
            this._modules.Set(this.Width - 3, 8, b12);

            var b11 = GetTypeFromBitPosition(bits, 11);
            this._modules.Set(8, 3, b11);
            this._modules.Set(this.Width - 4, 8, b11);

            var b10 = GetTypeFromBitPosition(bits, 10);
            this._modules.Set(8, 4, b10);
            this._modules.Set(this.Width - 5, 8, b10);

            var b09 = GetTypeFromBitPosition(bits, 9);
            this._modules.Set(8, 5, b09);
            this._modules.Set(this.Width - 6, 8, b09);

            var b08 = GetTypeFromBitPosition(bits, 8);
            this._modules.Set(8, 7, b08);
            this._modules.Set(this.Width - 7, 8, b08);
            this._modules.Set(this.Width - 8, 8, ModuleFlag.Dark); // dark module above bit 8 in bottom-left

            var b07 = GetTypeFromBitPosition(bits, 7);
            this._modules.Set(8, 8, b07);
            this._modules.Set(8, this.Width - 8, b07);

            var b06 = GetTypeFromBitPosition(bits, 6);
            this._modules.Set(7, 8, b06);
            this._modules.Set(8, this.Width - 7, b06);

            var b05 = GetTypeFromBitPosition(bits, 5);
            this._modules.Set(5, 8, b05);
            this._modules.Set(8, this.Width - 6, b05);

            var b04 = GetTypeFromBitPosition(bits, 4);
            this._modules.Set(4, 8, b04);
            this._modules.Set(8, this.Width - 5, b04);

            var b03 = GetTypeFromBitPosition(bits, 3);
            this._modules.Set(3, 8, b03);
            this._modules.Set(8, this.Width - 4, b03);

            var b02 = GetTypeFromBitPosition(bits, 2);
            this._modules.Set(2, 8, b02);
            this._modules.Set(8, this.Width - 3, b02);

            var b01 = GetTypeFromBitPosition(bits, 1);
            this._modules.Set(1, 8, b01);
            this._modules.Set(8, this.Width - 2, b01);

            var b00 = GetTypeFromBitPosition(bits, 0);
            this._modules.Set(0, 8, b00);
            this._modules.Set(8, this.Width - 1, b00);
        }

        private static ModuleFlag GetTypeFromBitPosition(int value, int bitPosition)
        {
            return ((value >> bitPosition) & 0x1) != 0 ? ModuleFlag.Dark : ModuleFlag.Light;
        }

        private static int GetBinaryIndicatorForErrorCorrectionLevel(ErrorToleranceLevel level)
        {
            // See ISO/IEC 18004:2006(E), Sec. 6.9.1, Table 12
            switch (level)
            {
                case ErrorToleranceLevel.VeryLow:
                    return 1;
                case ErrorToleranceLevel.Low:
                    return 0;
                case ErrorToleranceLevel.Medium:
                    return 3;
                case ErrorToleranceLevel.High:
                    return 2;
            }

            throw new InvalidOperationException();
        }

        private int SelectAndApplyMaskingPattern()
        {
            // Masks the QR code to minimize chance of data transmission error
            // See ISO/IEC 18004:2006(E), Sec. 6.8

            var candidates = new SymbolTemplate[_dataMaskPredicates.Length - 1];
            for (var i = 0; i < candidates.Length; i++)
            {
                var candidate = new SymbolTemplate(this);
                candidate.ApplyMask(_dataMaskPredicates[i]);
                candidates[i] = candidate;
            }

            var lowestPenaltyScoreValue = candidates[0].CalculatePenaltyScore();
            var lowestPenaltyScoreIndex = 0;
            for (var i = 1; i < candidates.Length; i++)
            {
                var candidatePenaltyScoreValue = candidates[i].CalculatePenaltyScore();
                if (candidatePenaltyScoreValue < lowestPenaltyScoreValue)
                {
                    lowestPenaltyScoreValue = candidatePenaltyScoreValue;
                    lowestPenaltyScoreIndex = i;
                }
            }

            // copy the candidate modules array into me; copy by ref is fine
            this._modules = candidates[lowestPenaltyScoreIndex]._modules;
            return lowestPenaltyScoreIndex;
        }

        private void ApplyMask(DataMaskPredicate predicate)
        {
            for (var row = 0; row < this.Width; row++)
            for (var col = 0; col < this.Width; col++)
                if (!this._modules.IsReserved(row, col) && predicate(row, col))
                {
                    this._modules.FlipColor(row, col);
                }
        }
    }
}
