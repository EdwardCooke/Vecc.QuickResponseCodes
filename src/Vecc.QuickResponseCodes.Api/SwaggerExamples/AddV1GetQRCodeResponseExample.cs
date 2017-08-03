using System.Text;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Vecc.QuickResponseCodes.Api.Models;

namespace Vecc.QuickResponseCodes.Api.SwaggerExamples
{
    public class AddV1GetQrCodeResponseExample : ISchemaFilter
    {
        public void Apply(Schema model, SchemaFilterContext context)
        {
            if (context.SystemType == typeof(Color))
            {
                model.Properties["red"].Example = 0;
                model.Properties["red"].Maximum = 255;
                model.Properties["red"].Minimum = 0;
                model.Properties["green"].Example = 0;
                model.Properties["green"].Maximum = 255;
                model.Properties["green"].Minimum = 0;
                model.Properties["blue"].Example = 0;
                model.Properties["blue"].Maximum = 255;
                model.Properties["blue"].Minimum = 0;
                model.Properties["alpha"].Example = 0;
                model.Properties["alpha"].Maximum = 255;
                model.Properties["alpha"].Minimum = 0;
            }

            if (context.SystemType == typeof(GetQrCodeRequest))
            {
                model.Properties["border"].Default = 4;
                model.Properties["border"].Description = "Width of whitespace around the code.";
                model.Properties["border"].Minimum = 4;

                model.Properties["data"].Description = "Base64 encoded byte array of the data.";

                model.Properties["dimensions"].Description = "Width/height of the image";
                model.Properties["dimensions"].Default = 100;

                model.Properties["errorToleranceLevel"].Default = "veryLow";

                model.Properties["imageFormat"].Default = "png";
                model.Properties["backgroundColor"].Default = "transparent";
                model.Properties["foregroundColor"].Default = "black";

                model.Example = new
                                {
                                    Data = Encoding.Default.GetBytes("TEST 123"),
                                    ErrorTolleranceLevel = "veryLow",
                                    ImageFormat = "png",
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
