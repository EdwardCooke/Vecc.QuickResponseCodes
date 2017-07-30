using System;
using System.Diagnostics;
using Vecc.QuickResponseCodes.Abstractions;

namespace Vecc.QuickResponseCodes.Internal.Models
{
    public struct CodeVersion
    {
        public readonly ErrorCorrectionInfo L;
        public readonly ErrorCorrectionInfo M;
        public readonly ErrorCorrectionInfo Q;
        public readonly ErrorCorrectionInfo H;

        public CodeVersion(ErrorCorrectionInfo l, ErrorCorrectionInfo m, ErrorCorrectionInfo q, ErrorCorrectionInfo h)
        {
            Debug.Assert(l.TotalBytes == m.TotalBytes && l.TotalBytes == q.TotalBytes && l.TotalBytes == h.TotalBytes);

            this.L = l;
            this.M = m;
            this.Q = q;
            this.H = h;
        }

        public ErrorCorrectionInfo GetCorrectionInfo(ErrorToleranceLevel level)
        {
            switch (level)
            {
                case ErrorToleranceLevel.VeryLow:
                    return this.L;
                case ErrorToleranceLevel.Low:
                    return this.M;
                case ErrorToleranceLevel.Medium:
                    return this.Q;
                case ErrorToleranceLevel.High:
                    return this.H;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level));
            }
        }
    }
}
