using System.ComponentModel.DataAnnotations;

namespace Vecc.QuickResponseCodes.Api.Models
{
    public class Color
    {
        [Required]
        public int Red { get; set; }
        [Required]
        public int Green { get; set; }
        [Required]
        public int Blue { get; set; }
        [Required]
        public int Alpha { get; set; }

        public Abstractions.Color ToAbstractions()
        {
            return new Abstractions.Color((byte)this.Red, (byte)this.Green, (byte)this.Blue, (byte)this.Alpha);
        }
    }
}
