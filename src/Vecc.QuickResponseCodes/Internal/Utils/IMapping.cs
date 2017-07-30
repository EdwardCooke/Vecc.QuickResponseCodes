using Vecc.QuickResponseCodes.Internal.Models;

namespace Vecc.QuickResponseCodes.Internal.Utils
{
    public interface IMapping
    {
        CodewordPlacement[] GetCodewordPlacements(ISymbolTemplate symbol);
    }
}
