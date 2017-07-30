namespace Vecc.QuickResponseCodes.Internal.Models
{
    internal struct FinderPatternDetector
    {
        private const int PatternA = 0x5D0; // 10111010000
        private const int PatternB = 0x5D; // 00001011101
        private const int Mask = 0x7FF; // 11111111111
        private const int NumberOfBitsRequiredForMask = 11;

        private int _numBitsShiftedIntoRegister;
        private int _currentRegister;

        public int NumFinderPatternsFound { get; private set; }

        public void ShiftIn(bool isDarkModule)
        {
            this._currentRegister = (this._currentRegister << 1) | (isDarkModule ? 1 : 0);
            if (++this._numBitsShiftedIntoRegister >= NumberOfBitsRequiredForMask)
            {
                var maskedRegisterValue = this._currentRegister & Mask;
                if (maskedRegisterValue == PatternA || maskedRegisterValue == PatternB)
                {
                    this.NumFinderPatternsFound++;
                }
            }
        }
    }
}
