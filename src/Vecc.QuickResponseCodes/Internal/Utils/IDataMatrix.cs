using System;

namespace Vecc.QuickResponseCodes.Internal.Utils
{
    public interface IDataMatrix : IDisposable
    {
        void WriteByte(byte b);
        void WriteBytes(ArraySegment<byte> bytes);
        void WriteNibble(byte value);
        void WriteWord(ushort value);
        byte[] ToArray();
    }
}
