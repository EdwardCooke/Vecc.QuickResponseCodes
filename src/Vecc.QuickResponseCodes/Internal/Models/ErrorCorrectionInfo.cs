namespace Vecc.QuickResponseCodes.Internal.Models
{
    public struct ErrorCorrectionInfo
    {
        public readonly BlockCountInfo[] BlockCountInfos;
        public readonly int TotalDataBytes;
        public readonly int TotalErrorBytes;
        public readonly int TotalBytes;

        public ErrorCorrectionInfo(params BlockCountInfo[] blockCountInfos)
        {
            this.BlockCountInfos = blockCountInfos;
            this.TotalDataBytes = 0;
            this.TotalErrorBytes = 0;

            checked
            {
                foreach (var blockCountInfo in blockCountInfos)
                {
                    this.TotalDataBytes += blockCountInfo.BlockCount * blockCountInfo.BlockInfo.DataBytes;
                    this.TotalErrorBytes += blockCountInfo.BlockCount * blockCountInfo.BlockInfo.ErrorBytes;
                }
            }

            this.TotalBytes = this.TotalDataBytes + this.TotalErrorBytes;
        }

        public ErrorCorrectionInfo(int blockCount, int totalBytes, int dataBytes)
            : this(new BlockCountInfo(blockCount, totalBytes, dataBytes))
        {
        }

        public ErrorCorrectionInfo(int blockCountA, int totalBytesA, int dataBytesA, int blockCountB, int totalBytesB,
                                   int dataBytesB)
            : this(new BlockCountInfo(blockCountA, totalBytesA, dataBytesA),
                   new BlockCountInfo(blockCountB, totalBytesB, dataBytesB))
        {
        }
    }
}
