namespace Vecc.QuickResponseCodes.Internal.Models
{
    public struct BlockInfo
    {
        public readonly int TotalBytes;
        public readonly int DataBytes;
        public readonly int ErrorBytes;

        public BlockInfo(int totalBytes, int dataBytes)
        {
            this.TotalBytes = totalBytes;
            this.DataBytes = dataBytes;
            this.ErrorBytes = totalBytes - dataBytes;
        }
    }
}
