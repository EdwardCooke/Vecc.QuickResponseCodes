using Vecc.QuickResponseCodes.Internal.Utils;

namespace Vecc.QuickResponseCodes.Internal.Factories
{
    public class DataMatrixFactory : IDataMatrixFactory
    {
        public IDataMatrix Create()
        {
            return new DataMatrix();
        }
    }
}
