using System.Drawing;

namespace Vecc.QuickResponseCodes.Internal.Models
{
    public struct CodewordPlacement
    {
        public readonly Point Bit7;
        public readonly Point Bit6;
        public readonly Point Bit5;
        public readonly Point Bit4;
        public readonly Point Bit3;
        public readonly Point Bit2;
        public readonly Point Bit1;
        public readonly Point Bit0;

        public CodewordPlacement(Point bit7, Point bit6, Point bit5, Point bit4,
                                 Point bit3, Point bit2, Point bit1, Point bit0)
        {
            this.Bit7 = bit7;
            this.Bit6 = bit6;
            this.Bit5 = bit5;
            this.Bit4 = bit4;
            this.Bit3 = bit3;
            this.Bit2 = bit2;
            this.Bit1 = bit1;
            this.Bit0 = bit0;
        }
    }
}
