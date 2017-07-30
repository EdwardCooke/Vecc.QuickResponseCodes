using System.Threading;
using Vecc.QuickResponseCodes.Internal.Utils;

namespace Vecc.QuickResponseCodes.Internal.Factories
{
    public class SymbolTemplateFactory : ISymbolTemplateFactory
    {
        private static readonly SymbolTemplate[] _symbolTemplates = new SymbolTemplate[40];
        private readonly IAlignmentPattern _alignmentPattern;
        private readonly IMapping _mapping;

        public SymbolTemplateFactory(IAlignmentPattern alignmentPattern, IMapping mapping)
        {
            this._alignmentPattern = alignmentPattern;
            this._mapping = mapping;
        }

        public ISymbolTemplate Create(int version)
        {
            // Lazy initialization of this symbol template
            var template = Volatile.Read(ref _symbolTemplates[version - 1]);
            if (template != null)
            {
                return new SymbolTemplate(template);
            }

            var newTemplate = new SymbolTemplate(this._alignmentPattern, this._mapping, version);
            template = Interlocked.CompareExchange(ref _symbolTemplates[version - 1], newTemplate, null) ?? newTemplate;

            // Return a copy since the templates are mutable
            return new SymbolTemplate(template);
        }
    }
}
