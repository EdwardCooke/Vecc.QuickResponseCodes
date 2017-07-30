namespace Vecc.QuickResponseCodes.Api.Models
{
    public class Color
    {
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }
        public byte Alpha { get; set; }

        public Abstractions.Color ToAbstractions()
        {
            return new Abstractions.Color(this.Red, this.Green, this.Blue, this.Alpha);
        }
    }
}
