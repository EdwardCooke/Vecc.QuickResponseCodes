using Vecc.QuickResponseCodes.Internal.Utils;

namespace Vecc.QuickResponseCodes.Internal.Factories
{
    public interface IGaloisFieldFactory
    {
        IGaloisField Create(uint polynomial);
    }
}
