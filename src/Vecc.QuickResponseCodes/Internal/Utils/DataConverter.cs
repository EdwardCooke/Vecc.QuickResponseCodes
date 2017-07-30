using System;

namespace Vecc.QuickResponseCodes.Internal.Utils
{
    public class DataConverter : IDataConverter
    {
        public void CopyToArray<T>(ArraySegment<T> input, T[] destination)
        {
            Buffer.BlockCopy(input.Array, input.Offset, destination, 0, input.Count);
        }
    }
}
