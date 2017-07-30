using System;

namespace Vecc.QuickResponseCodes.Internal.Enums
{
    [Flags]
    internal enum ModuleFlag
    {
        /// <summary>
        ///     Module is not set
        /// </summary>
        Light = 0,

        /// <summary>
        ///     Module is set
        /// </summary>
        Dark = 1,

        /// <summary>
        ///     Module is a reserved pixel and not usable by data
        /// </summary>
        Reserved = 2
    }
}
