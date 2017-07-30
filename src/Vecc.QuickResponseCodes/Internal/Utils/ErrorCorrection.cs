using System;
using System.Collections.Generic;
using System.IO;
using Vecc.QuickResponseCodes.Abstractions;
using Vecc.QuickResponseCodes.Internal.Exceptions;
using Vecc.QuickResponseCodes.Internal.Models;

namespace Vecc.QuickResponseCodes.Internal.Utils
{
    public class ErrorCorrection : IErrorCorrection
    {
        private static readonly CodeVersion[] _versions;
        private readonly IReedSolomon _reedSolomon;

        static ErrorCorrection()
        {
            _versions = GetVersions();
        }

        public ErrorCorrection(IReedSolomon reedSolomon)
        {
            this._reedSolomon = reedSolomon;
        }

        public CodeVersion GetQuickResponseCodeVersionInfo(int version)
        {
            return _versions[version - 1];
        }

        public ErrorCorrectionInfo GetErrorCorrectionInfo(int inputBytes,
                                                          ErrorToleranceLevel desiredLevel,
                                                          out int qrCodeVersion,
                                                          out ErrorToleranceLevel errorToleranceLevel)
        {
            for (var i = 0; i < _versions.Length; i++)
            {
                var version = _versions[i];
                if (version.GetCorrectionInfo(desiredLevel).TotalDataBytes < inputBytes)
                {
                    continue;
                }

                // Found match, now get highest correction code within this version that can still hold desired number of bytes.
                qrCodeVersion = i + 1;

                if (version.H.TotalDataBytes >= inputBytes)
                {
                    errorToleranceLevel = ErrorToleranceLevel.High;
                    return version.H;
                }

                if (version.Q.TotalDataBytes >= inputBytes)
                {
                    errorToleranceLevel = ErrorToleranceLevel.Medium;
                    return version.Q;
                }

                if (version.M.TotalDataBytes >= inputBytes)
                {
                    errorToleranceLevel = ErrorToleranceLevel.Low;
                    return version.M;
                }

                errorToleranceLevel = ErrorToleranceLevel.VeryLow;
                return version.L;
            }

            throw new InputTooLongException();
        }

        public byte[] GetMessageSequence(byte[] input,
                                         ErrorToleranceLevel desiredLevel,
                                         out int qrCodeVersion,
                                         out ErrorToleranceLevel errorToleranceLevel)
        {
            // See ISO/IEC 18004:2006(E), Sec. 6.6 for information on generating the final message sequence

            var correctionInfo = this.GetErrorCorrectionInfo(input.Length, desiredLevel, out qrCodeVersion, out errorToleranceLevel);
            var extendedInput = new byte[correctionInfo.TotalDataBytes];
            Buffer.BlockCopy(input, 0, extendedInput, 0, input.Length);

            // generate data and error blocks
            var bytesCopiedSoFar = 0;
            var dataBlocks = new List<byte[]>();
            var errorBlocks = new List<byte[]>();

            foreach (var blockCountInfo in correctionInfo.BlockCountInfos)
            {
                for (var i = 0; i < blockCountInfo.BlockCount; i++)
                {
                    // create a data block
                    var thisDataBlock = new byte[blockCountInfo.BlockInfo.DataBytes];
                    Buffer.BlockCopy(extendedInput, bytesCopiedSoFar, thisDataBlock, 0, thisDataBlock.Length);
                    dataBlocks.Add(thisDataBlock);
                    bytesCopiedSoFar += thisDataBlock.Length;

                    // create an error block
                    var thisErrorBlock = this._reedSolomon.GetErrorCorrectionBytes(thisDataBlock, blockCountInfo.BlockInfo.ErrorBytes);
                    errorBlocks.Add(thisErrorBlock);
                }
            }

            // Assemble the final message sequence
            using (var ms = new MemoryStream(correctionInfo.TotalDataBytes + correctionInfo.TotalErrorBytes))
            {
                FlushBlocks(dataBlocks, ms);
                FlushBlocks(errorBlocks, ms);
                return ms.ToArray();
            }
        }

        private static CodeVersion[] GetVersions()
        {
            // See ISO/IEC 18004:2006(E), Table 9
            var versions = new CodeVersion[40];

            // Version 1
            versions[0] = new CodeVersion(new ErrorCorrectionInfo(1, 26, 19),
                                          new ErrorCorrectionInfo(1, 26, 16),
                                          new ErrorCorrectionInfo(1, 26, 13),
                                          new ErrorCorrectionInfo(1, 26, 9));

            // Version 2
            versions[1] = new CodeVersion(new ErrorCorrectionInfo(1, 44, 34),
                                          new ErrorCorrectionInfo(1, 44, 28),
                                          new ErrorCorrectionInfo(1, 44, 22),
                                          new ErrorCorrectionInfo(1, 44, 16));

            // Version 3
            versions[2] = new CodeVersion(new ErrorCorrectionInfo(1, 70, 55),
                                          new ErrorCorrectionInfo(1, 70, 44),
                                          new ErrorCorrectionInfo(2, 35, 17),
                                          new ErrorCorrectionInfo(2, 35, 13));

            // Version 4
            versions[3] = new CodeVersion(new ErrorCorrectionInfo(1, 100, 80),
                                          new ErrorCorrectionInfo(2, 50, 32),
                                          new ErrorCorrectionInfo(2, 50, 24),
                                          new ErrorCorrectionInfo(4, 25, 9));

            // Version 5
            versions[4] = new CodeVersion(new ErrorCorrectionInfo(1, 134, 108),
                                          new ErrorCorrectionInfo(2, 67, 43),
                                          new ErrorCorrectionInfo(2, 33, 15, 2, 34, 16),
                                          new ErrorCorrectionInfo(2, 33, 11, 2, 34, 12));

            // Version 6
            versions[5] = new CodeVersion(new ErrorCorrectionInfo(2, 86, 68),
                                          new ErrorCorrectionInfo(4, 43, 27),
                                          new ErrorCorrectionInfo(4, 43, 19),
                                          new ErrorCorrectionInfo(4, 43, 15));

            // Version 7
            versions[6] = new CodeVersion(new ErrorCorrectionInfo(2, 98, 78),
                                          new ErrorCorrectionInfo(4, 49, 31),
                                          new ErrorCorrectionInfo(2, 32, 14, 4, 33, 15),
                                          new ErrorCorrectionInfo(4, 39, 13, 1, 40, 14));

            // Version 8
            versions[7] = new CodeVersion(new ErrorCorrectionInfo(2, 121, 97),
                                          new ErrorCorrectionInfo(2, 60, 38, 2, 61, 39),
                                          new ErrorCorrectionInfo(4, 40, 18, 2, 41, 19),
                                          new ErrorCorrectionInfo(4, 40, 14, 2, 41, 15));

            // Version 9
            versions[8] = new CodeVersion(new ErrorCorrectionInfo(2, 146, 116),
                                          new ErrorCorrectionInfo(3, 58, 36, 2, 59, 37),
                                          new ErrorCorrectionInfo(4, 36, 16, 4, 37, 17),
                                          new ErrorCorrectionInfo(4, 36, 12, 4, 37, 13));

            // Version 10
            versions[9] = new CodeVersion(new ErrorCorrectionInfo(2, 86, 68, 2, 87, 69),
                                          new ErrorCorrectionInfo(4, 69, 43, 1, 70, 44),
                                          new ErrorCorrectionInfo(6, 43, 19, 2, 44, 20),
                                          new ErrorCorrectionInfo(6, 43, 15, 2, 44, 16));

            // Version 11
            versions[10] = new CodeVersion(new ErrorCorrectionInfo(4, 101, 81),
                                           new ErrorCorrectionInfo(1, 80, 50, 4, 81, 51),
                                           new ErrorCorrectionInfo(4, 50, 22, 4, 51, 23),
                                           new ErrorCorrectionInfo(3, 36, 12, 8, 37, 13));

            // Version 12
            versions[11] = new CodeVersion(new ErrorCorrectionInfo(2, 116, 92, 2, 117, 93),
                                           new ErrorCorrectionInfo(6, 58, 36, 2, 59, 37),
                                           new ErrorCorrectionInfo(4, 46, 20, 6, 47, 21),
                                           new ErrorCorrectionInfo(7, 42, 14, 4, 43, 15));

            // Version 13
            versions[12] = new CodeVersion(new ErrorCorrectionInfo(2, 116, 92, 2, 117, 93),
                                           new ErrorCorrectionInfo(6, 58, 36, 2, 59, 37),
                                           new ErrorCorrectionInfo(4, 46, 20, 6, 47, 21),
                                           new ErrorCorrectionInfo(7, 42, 14, 4, 43, 15));

            // Version 14
            versions[13] = new CodeVersion(new ErrorCorrectionInfo(3, 145, 115, 1, 146, 116),
                                           new ErrorCorrectionInfo(4, 64, 40, 5, 65, 41),
                                           new ErrorCorrectionInfo(11, 36, 16, 5, 37, 17),
                                           new ErrorCorrectionInfo(11, 36, 12, 5, 37, 13));

            // Version 15
            versions[14] = new CodeVersion(new ErrorCorrectionInfo(5, 109, 87, 1, 110, 88),
                                           new ErrorCorrectionInfo(5, 65, 41, 5, 66, 42),
                                           new ErrorCorrectionInfo(5, 54, 24, 7, 55, 25),
                                           new ErrorCorrectionInfo(11, 36, 12, 7, 37, 13));

            // Version 16
            versions[15] = new CodeVersion(new ErrorCorrectionInfo(5, 122, 98, 1, 123, 99),
                                           new ErrorCorrectionInfo(7, 73, 45, 3, 74, 46),
                                           new ErrorCorrectionInfo(15, 43, 19, 2, 44, 20),
                                           new ErrorCorrectionInfo(3, 45, 15, 13, 46, 16));

            // Version 17
            versions[16] = new CodeVersion(new ErrorCorrectionInfo(1, 135, 107, 5, 136, 108),
                                           new ErrorCorrectionInfo(10, 74, 46, 1, 75, 47),
                                           new ErrorCorrectionInfo(1, 50, 22, 15, 51, 23),
                                           new ErrorCorrectionInfo(2, 42, 14, 17, 43, 15));

            // Version 18
            versions[17] = new CodeVersion(new ErrorCorrectionInfo(5, 150, 120, 1, 151, 121),
                                           new ErrorCorrectionInfo(9, 69, 43, 4, 70, 44),
                                           new ErrorCorrectionInfo(17, 50, 22, 1, 51, 23),
                                           new ErrorCorrectionInfo(2, 42, 14, 19, 43, 15));

            // Version 19
            versions[18] = new CodeVersion(new ErrorCorrectionInfo(3, 141, 113, 4, 142, 114),
                                           new ErrorCorrectionInfo(3, 70, 44, 11, 71, 45),
                                           new ErrorCorrectionInfo(17, 47, 21, 4, 48, 22),
                                           new ErrorCorrectionInfo(9, 39, 13, 16, 40, 14));

            // Version 20
            versions[19] = new CodeVersion(new ErrorCorrectionInfo(3, 135, 107, 5, 136, 108),
                                           new ErrorCorrectionInfo(3, 67, 41, 13, 68, 42),
                                           new ErrorCorrectionInfo(15, 54, 24, 5, 55, 25),
                                           new ErrorCorrectionInfo(15, 43, 15, 10, 44, 16));

            // Version 21
            versions[20] = new CodeVersion(new ErrorCorrectionInfo(4, 144, 116, 4, 145, 117),
                                           new ErrorCorrectionInfo(17, 68, 42),
                                           new ErrorCorrectionInfo(17, 50, 22, 6, 51, 23),
                                           new ErrorCorrectionInfo(19, 46, 16, 6, 47, 17));

            // Version 21
            versions[21] = new CodeVersion(new ErrorCorrectionInfo(2, 139, 111, 7, 140, 112),
                                           new ErrorCorrectionInfo(17, 74, 46),
                                           new ErrorCorrectionInfo(7, 54, 24, 16, 55, 25),
                                           new ErrorCorrectionInfo(34, 37, 13));

            // Version 23
            versions[22] = new CodeVersion(new ErrorCorrectionInfo(4, 151, 121, 5, 152, 122),
                                           new ErrorCorrectionInfo(4, 75, 47, 14, 76, 48),
                                           new ErrorCorrectionInfo(11, 54, 24, 14, 55, 25),
                                           new ErrorCorrectionInfo(16, 45, 15, 14, 46, 16));

            // Version 24
            versions[23] = new CodeVersion(new ErrorCorrectionInfo(6, 147, 117, 4, 148, 118),
                                           new ErrorCorrectionInfo(6, 73, 45, 14, 74, 46),
                                           new ErrorCorrectionInfo(11, 54, 24, 16, 55, 25),
                                           new ErrorCorrectionInfo(30, 46, 16, 2, 47, 17));

            // Version 25
            versions[24] = new CodeVersion(new ErrorCorrectionInfo(8, 132, 106, 4, 133, 107),
                                           new ErrorCorrectionInfo(8, 75, 47, 13, 76, 48),
                                           new ErrorCorrectionInfo(7, 54, 24, 22, 55, 25),
                                           new ErrorCorrectionInfo(22, 45, 15, 13, 46, 16));

            // Version 26
            versions[25] = new CodeVersion(new ErrorCorrectionInfo(10, 142, 114, 2, 143, 115),
                                           new ErrorCorrectionInfo(19, 74, 46, 4, 75, 47),
                                           new ErrorCorrectionInfo(28, 50, 22, 6, 51, 23),
                                           new ErrorCorrectionInfo(33, 46, 16, 4, 47, 17));

            // Version 27
            versions[26] = new CodeVersion(new ErrorCorrectionInfo(8, 152, 122, 4, 153, 123),
                                           new ErrorCorrectionInfo(22, 73, 45, 3, 74, 46),
                                           new ErrorCorrectionInfo(8, 53, 23, 26, 54, 24),
                                           new ErrorCorrectionInfo(12, 45, 15, 28, 46, 16));

            // Version 28
            versions[27] = new CodeVersion(new ErrorCorrectionInfo(3, 147, 117, 10, 148, 118),
                                           new ErrorCorrectionInfo(3, 73, 45, 23, 74, 46),
                                           new ErrorCorrectionInfo(4, 54, 24, 31, 55, 25),
                                           new ErrorCorrectionInfo(11, 45, 15, 31, 46, 16));

            // Version 29
            versions[28] = new CodeVersion(new ErrorCorrectionInfo(7, 146, 116, 7, 147, 117),
                                           new ErrorCorrectionInfo(21, 73, 45, 7, 74, 46),
                                           new ErrorCorrectionInfo(1, 53, 23, 37, 54, 24),
                                           new ErrorCorrectionInfo(19, 45, 15, 26, 46, 16));

            // Version 30
            versions[29] = new CodeVersion(new ErrorCorrectionInfo(5, 145, 115, 10, 146, 116),
                                           new ErrorCorrectionInfo(19, 75, 47, 10, 76, 48),
                                           new ErrorCorrectionInfo(15, 54, 24, 25, 55, 25),
                                           new ErrorCorrectionInfo(23, 45, 15, 25, 46, 16));

            // Version 31
            versions[30] = new CodeVersion(new ErrorCorrectionInfo(13, 145, 115, 3, 146, 116),
                                           new ErrorCorrectionInfo(2, 74, 46, 29, 75, 47),
                                           new ErrorCorrectionInfo(42, 54, 24, 1, 55, 25),
                                           new ErrorCorrectionInfo(23, 45, 15, 28, 46, 16));

            // Version 32
            versions[31] = new CodeVersion(new ErrorCorrectionInfo(17, 145, 115),
                                           new ErrorCorrectionInfo(10, 74, 46, 23, 75, 47),
                                           new ErrorCorrectionInfo(10, 54, 24, 35, 55, 25),
                                           new ErrorCorrectionInfo(19, 45, 15, 35, 46, 16));

            // Version 33
            versions[32] = new CodeVersion(new ErrorCorrectionInfo(17, 145, 115, 1, 146, 116),
                                           new ErrorCorrectionInfo(14, 74, 46, 21, 75, 47),
                                           new ErrorCorrectionInfo(29, 54, 24, 19, 55, 25),
                                           new ErrorCorrectionInfo(11, 45, 15, 46, 46, 16));

            // Version 34
            versions[33] = new CodeVersion(new ErrorCorrectionInfo(13, 145, 115, 6, 146, 116),
                                           new ErrorCorrectionInfo(14, 74, 46, 23, 75, 47),
                                           new ErrorCorrectionInfo(44, 54, 24, 7, 55, 25),
                                           new ErrorCorrectionInfo(59, 46, 16, 1, 47, 17));

            // Version 15
            versions[34] = new CodeVersion(new ErrorCorrectionInfo(12, 151, 121, 7, 152, 122),
                                           new ErrorCorrectionInfo(12, 75, 47, 26, 76, 48),
                                           new ErrorCorrectionInfo(39, 54, 24, 14, 55, 25),
                                           new ErrorCorrectionInfo(22, 45, 15, 41, 46, 16));

            // Version 36
            versions[35] = new CodeVersion(new ErrorCorrectionInfo(6, 151, 121, 14, 152, 122),
                                           new ErrorCorrectionInfo(6, 75, 47, 34, 76, 48),
                                           new ErrorCorrectionInfo(46, 54, 24, 10, 55, 25),
                                           new ErrorCorrectionInfo(2, 45, 15, 64, 46, 16));

            // Version 37
            versions[36] = new CodeVersion(new ErrorCorrectionInfo(17, 152, 122, 4, 153, 123),
                                           new ErrorCorrectionInfo(29, 74, 46, 14, 75, 47),
                                           new ErrorCorrectionInfo(49, 54, 24, 10, 55, 25),
                                           new ErrorCorrectionInfo(24, 45, 15, 46, 46, 16));

            // Version 38
            versions[37] = new CodeVersion(new ErrorCorrectionInfo(4, 152, 122, 18, 153, 123),
                                           new ErrorCorrectionInfo(13, 74, 46, 32, 75, 47),
                                           new ErrorCorrectionInfo(48, 54, 24, 14, 55, 25),
                                           new ErrorCorrectionInfo(42, 45, 15, 32, 46, 16));

            // Version 39
            versions[38] = new CodeVersion(new ErrorCorrectionInfo(20, 147, 117, 4, 148, 118),
                                           new ErrorCorrectionInfo(40, 75, 47, 7, 76, 48),
                                           new ErrorCorrectionInfo(43, 54, 24, 22, 55, 25),
                                           new ErrorCorrectionInfo(10, 45, 15, 67, 46, 16));

            // Version 40
            versions[39] = new CodeVersion(new ErrorCorrectionInfo(19, 148, 118, 6, 149, 119),
                                           new ErrorCorrectionInfo(18, 75, 47, 31, 76, 48),
                                           new ErrorCorrectionInfo(34, 54, 24, 34, 55, 25),
                                           new ErrorCorrectionInfo(20, 45, 15, 61, 46, 16));

            return versions;
        }

        private static void FlushBlocks(IList<byte[]> blocks, Stream stream)
        {
            for (var i = 0;; i++)
            {
                var wroteBlock = false;
                foreach (var thisBlock in blocks)
                {
                    if (thisBlock.Length <= i)
                    {
                        continue;
                    }

                    wroteBlock = true;
                    stream.WriteByte(thisBlock[i]);
                }

                if (!wroteBlock)
                {
                    return;
                }
            }
        }
    }
}
