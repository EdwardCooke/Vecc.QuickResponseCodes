using System.Collections.Generic;
using System.Drawing;

namespace Vecc.QuickResponseCodes.Internal.Utils
{
    public interface IAlignmentPattern
    {
        IEnumerable<Point> GetCoordinates(int version);
    }
}
