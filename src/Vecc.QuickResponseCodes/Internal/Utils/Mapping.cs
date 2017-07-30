using System.Drawing;
using Vecc.QuickResponseCodes.Internal.Models;

namespace Vecc.QuickResponseCodes.Internal.Utils
{
    public class Mapping : IMapping
    {
        public CodewordPlacement[] GetCodewordPlacements(ISymbolTemplate symbol)
        {
            var bitstreamMapping = this.GetBitStreamMapping(symbol);
            return this.GetCodewordPlacementsImpl(bitstreamMapping);
        }

        private CodewordPlacement[] GetCodewordPlacementsImpl(Point[] bitstreamMapping)
        {
            var result = new CodewordPlacement[bitstreamMapping.Length / 8];

            for (var position = 0; position < bitstreamMapping.Length - 8; position += 8)
                result[position / 8] = new CodewordPlacement(bitstreamMapping[position],
                                                             bitstreamMapping[position + 1],
                                                             bitstreamMapping[position + 2],
                                                             bitstreamMapping[position + 3],
                                                             bitstreamMapping[position + 4],
                                                             bitstreamMapping[position + 5],
                                                             bitstreamMapping[position + 6],
                                                             bitstreamMapping[position + 7]);

            return result;
        }

        private Point[] GetBitStreamMapping(ISymbolTemplate symbol)
        {
            // See ISO/IEC 18004:2006(E), Sec. 6.7.3 for information on where codewords
            // are placed in the matrix.

            var width = symbol.Width;
            var result = new Point[width * width];

            var isMovingUpward = true;
            var position = 0;
            for (var col = width - 1; col > 0; col -= 2)
            {
                // Column six is used as a timing pattern and is skipped for codeword placement
                if (col == 6)
                {
                    col--;
                }

                if (isMovingUpward)
                {
                    for (var row = width - 1; row >= 0; row--)
                    {
                        if (!symbol.IsReservedModule(row, col))
                        {
                            result[position] = new Point(col, row);
                            position++;
                        }
                        if (!symbol.IsReservedModule(row, col - 1))
                        {
                            result[position] = new Point(col - 1, row);
                            position++;
                        }
                    }
                }
                else
                {
                    for (var row = 0; row < width; row++)
                    {
                        if (!symbol.IsReservedModule(row, col))
                        {
                            result[position] = new Point(col, row);
                            position++;
                        }

                        if (!symbol.IsReservedModule(row, col - 1))
                        {
                            result[position] = new Point(col - 1, row);
                            position++;
                        }
                    }
                }

                isMovingUpward = !isMovingUpward; // reverse direction when hitting the end of the matrix
            }

            return result;
        }
    }
}
