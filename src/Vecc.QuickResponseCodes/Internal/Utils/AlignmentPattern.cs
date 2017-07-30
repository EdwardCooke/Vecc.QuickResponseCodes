using System.Collections.Generic;
using System.Drawing;

namespace Vecc.QuickResponseCodes.Internal.Utils
{
    public class AlignmentPattern : IAlignmentPattern
    {
        private static readonly int[][] _coordinateMappings;

        static AlignmentPattern()
        {
            _coordinateMappings = GetLookupTable();
        }

        public IEnumerable<Point> GetCoordinates(int version)
        {
            var coordinateMapping = _coordinateMappings[version - 1];
            var result = new List<Point>();

            for (var x = 0; x < coordinateMapping.Length; x++)
            for (var y = 0; y < coordinateMapping.Length; y++)
            {
                // We don't return mappings for the top-left, top-right, and bottom-left alignment patterns
                if (x == 0 && y == 0 || //top-left
                    x == coordinateMapping.Length - 1 && y == 0 || //top-right
                    x == 0 && y == coordinateMapping.Length - 1) // bottom-left
                {
                    continue;
                }

                result.Add(new Point(coordinateMapping[x], coordinateMapping[y]));
            }

            return result;
        }

        private static int[][] GetLookupTable()
        {
            // Given by ISO/IEC 18004:2006(E), Annex E
            var coords = new int[40][];

            coords[0] = new int[] { };
            coords[1] = new[] {6, 18};
            coords[2] = new[] {6, 22};
            coords[3] = new[] {6, 26};
            coords[4] = new[] {6, 30};
            coords[5] = new[] {6, 34};
            coords[6] = new[] {6, 22, 38};
            coords[7] = new[] {6, 24, 42};
            coords[8] = new[] {6, 26, 46};
            coords[9] = new[] {6, 28, 50};
            coords[10] = new[] {6, 30, 54};
            coords[11] = new[] {6, 32, 58};
            coords[12] = new[] {6, 34, 62};
            coords[13] = new[] {6, 26, 46, 66};
            coords[14] = new[] {6, 26, 48, 70};
            coords[15] = new[] {6, 26, 50, 74};
            coords[16] = new[] {6, 30, 54, 78};
            coords[17] = new[] {6, 30, 56, 82};
            coords[18] = new[] {6, 30, 58, 86};
            coords[19] = new[] {6, 34, 62, 90};
            coords[20] = new[] {6, 28, 50, 72, 94};
            coords[21] = new[] {6, 26, 50, 74, 98};
            coords[22] = new[] {6, 30, 54, 78, 102};
            coords[23] = new[] {6, 28, 54, 80, 106};
            coords[24] = new[] {6, 32, 58, 84, 110};
            coords[25] = new[] {6, 30, 58, 86, 114};
            coords[26] = new[] {6, 34, 62, 90, 118};
            coords[27] = new[] {6, 26, 50, 74, 98, 122};
            coords[28] = new[] {6, 30, 54, 78, 102, 126};
            coords[29] = new[] {6, 26, 52, 78, 104, 130};
            coords[30] = new[] {6, 30, 56, 82, 108, 134};
            coords[31] = new[] {6, 34, 60, 86, 112, 138};
            coords[32] = new[] {6, 30, 58, 86, 114, 142};
            coords[33] = new[] {6, 34, 62, 90, 118, 146};
            coords[34] = new[] {6, 30, 54, 78, 102, 126, 150};
            coords[35] = new[] {6, 24, 50, 76, 102, 128, 154};
            coords[36] = new[] {6, 28, 54, 80, 106, 132, 158};
            coords[37] = new[] {6, 32, 58, 84, 110, 136, 162};
            coords[38] = new[] {6, 26, 54, 82, 110, 138, 166};
            coords[39] = new[] {6, 30, 58, 86, 114, 142, 170};

            return coords;
        }
    }
}
