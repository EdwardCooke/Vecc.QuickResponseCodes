using System;

namespace Vecc.QuickResponseCodes.Internal.Exceptions
{
    public class InputTooLongException : Exception
    {
        public InputTooLongException()
        {
        }

        public InputTooLongException(string message)
            : base(message)
        {
        }

        public InputTooLongException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
