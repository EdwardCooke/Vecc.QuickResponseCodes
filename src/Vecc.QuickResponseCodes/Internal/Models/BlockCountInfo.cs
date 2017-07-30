namespace Vecc.QuickResponseCodes.Internal.Models
{
    public struct BlockCountInfo
    {
        public readonly int BlockCount;
        public readonly BlockInfo BlockInfo;

        public BlockCountInfo(int blockCount, int totalBytes, int dataBytes)
        {
            this.BlockCount = blockCount;
            this.BlockInfo = new BlockInfo(totalBytes, dataBytes);
        }
    }
}
