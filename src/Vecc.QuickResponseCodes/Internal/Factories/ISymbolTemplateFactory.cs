using Vecc.QuickResponseCodes.Internal.Utils;

namespace Vecc.QuickResponseCodes.Internal.Factories
{
    public interface ISymbolTemplateFactory
    {
        ISymbolTemplate Create(int version);
    }
}
