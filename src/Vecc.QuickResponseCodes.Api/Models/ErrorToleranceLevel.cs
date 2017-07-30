namespace Vecc.QuickResponseCodes.Api.Models
{
    /// <summary>
    ///     Error tolerances
    /// </summary>
    public enum ErrorToleranceLevel
    {
        /// <summary>
        ///     Allows 7% data loss.
        /// </summary>
        VeryLow = 0,

        /// <summary>
        ///     Allows 15% data loss.
        /// </summary>
        Low = 1,

        /// <summary>
        ///     Allows 25% data loss.
        /// </summary>
        Medium = 2,

        /// <summary>
        ///     Allows 30% data loss.
        /// </summary>
        High = 3
    }
}
