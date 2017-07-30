using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Vecc.QuickResponseCodes.Api.Models;

namespace Vecc.QuickResponseCodes.Api.SwaggerExamples
{
    public class AddV1GetQrCodeResponseExample : ISchemaFilter
    {
        public void Apply(Schema model, SchemaFilterContext context)
        {
            if (context.SystemType == typeof(GetQrCodeRequest))
            {
                //use an anonymous type to get around the enum's showing as integer values and not the string representations. Hacky, but it works.
                model.Example = new
                                {
                                    Data = "TEST 123",
                                    ErrorTolleranceLevel = ErrorToleranceLevel.VeryLow.ToString(),
                                    ImageFormat = CodeImageFormat.Png.ToString(),
                                    BackgroundColor = new
                                    {
                                        Red = 0,
                                        Green = 0,
                                        Blue = 0,
                                        Alpha = 0
                                    },
                                    ForegroundColor = new
                                    {
                                        Red = 0,
                                        Green = 0,
                                        Blue = 0,
                                        Alpha = 255
                                    },
                                    Dimensions = 100,
                                    Border = 4
                                };
            }
        }
    }
}
