using Vecc.QuickResponseCodes.Abstractions;
using Vecc.QuickResponseCodes.Internal;
using Vecc.QuickResponseCodes.Internal.Factories;
using Vecc.QuickResponseCodes.Internal.Utils;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class QRCodeGeneratorServiceCollectionExtensions
    {
        public static IServiceCollection AddQuickResponseCodes(this IServiceCollection serviceCollection)
        {
            //Core
            serviceCollection.AddScoped<IQuickResponseCodeGenerator, QuickResponseGenerator>();

            //Factories
            serviceCollection.AddScoped<IDataMatrixFactory, DataMatrixFactory>();
            serviceCollection.AddScoped<IGaloisFieldFactory, GaloisFieldFactory>();
            serviceCollection.AddScoped<ISymbolTemplateFactory, SymbolTemplateFactory>();

            //Utilities
            serviceCollection.AddScoped<IAlignmentPattern, AlignmentPattern>();
            serviceCollection.AddScoped<IDataConverter, DataConverter>();
            serviceCollection.AddScoped<IDataMatrix, DataMatrix>();
            serviceCollection.AddScoped<IErrorCorrection, ErrorCorrection>();
            serviceCollection.AddScoped<IGaloisField, GaloisField>();
            serviceCollection.AddScoped<IMapping, Mapping>();
            serviceCollection.AddScoped<IReedSolomon, ReedSolomon>();
            serviceCollection.AddScoped<ISymbolTemplate, SymbolTemplate>();

            return serviceCollection;
        }
    }
}
