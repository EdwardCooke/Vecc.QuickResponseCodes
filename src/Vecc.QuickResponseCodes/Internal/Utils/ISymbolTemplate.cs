using Drawing = System.Drawing;
using Vecc.QuickResponseCodes.Abstractions;

namespace Vecc.QuickResponseCodes.Internal.Utils
{
    public interface ISymbolTemplate
    {
        ErrorToleranceLevel ErrorToleranceLevel { get; set; }
        int Width { get; }

        void Complete();
        bool IsReservedModule(int row, int col);
        void PopulateData(byte[] rawData);
        Drawing.Image ToImage(int moduleSize, int borderWidth, Drawing.Color background, Drawing.Color foreground);
    }
}
