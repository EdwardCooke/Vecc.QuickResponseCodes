using System;

namespace Vecc.QuickResponseCodes.Internal.Utils
{
    public interface IDataConverter
    {
        void CopyToArray<T>(ArraySegment<T> input, T[] destination);
    }
}
