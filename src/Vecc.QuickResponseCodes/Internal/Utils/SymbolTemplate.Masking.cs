using System;
using Vecc.QuickResponseCodes.Internal.Models;

namespace Vecc.QuickResponseCodes.Internal.Utils
{
    public sealed partial class SymbolTemplate
    {
        private static readonly DataMaskPredicate[] _dataMaskPredicates =
        {
            (row, col) => (row + col) % 2 == 0, // 000
            (row, col) => row % 2 == 0, // 001
            (row, col) => col % 3 == 0, // 010
            (row, col) => (row + col) % 3 == 0, // 011
            (row, col) => (row / 2 + col / 3) % 2 == 0, // 100
            (row, col) =>
            {
                var prod = row * col;
                return prod % 2 + prod % 3 == 0;
            }, // 101
            (row, col) =>
            {
                var prod = row * col;
                return (prod % 2 + prod % 3) % 2 == 0;
            }, // 110
            (row, col) => ((row + col) % 2 + row * col % 3) % 2 == 0 // 111
        };

        private int CalculatePenaltyScore()
        {
            var adjacentModulePenalty = this.CalculatePenaltyScoreA1();
            var homogenousBlockPenalty = this.CalculatePenaltyScoreB();
            var finderPatternPenalty = this.CalculatePenaltyScoreC();
            var unbalancedProportionPenalty = this.CalculatePenaltyScoreD();
            return adjacentModulePenalty + homogenousBlockPenalty + finderPatternPenalty + unbalancedProportionPenalty;
        }

        // Adjacent modules in row / column in same color
        // Eval condition: # modules = 5 + i
        // Points: 3 + i
        private int CalculatePenaltyScoreA1()
        {
            var totalScore = 0;
            for (var i = 0; i < this.Width; i++)
            {
                totalScore += this.CalculatePenaltyScoreA1ForRow(i); // first by row
                totalScore += this.CalculatePenaltyScoreA1ForCol(i); // then by col
            }
            return totalScore;
        }

        private int CalculatePenaltyScoreA1ForRow(int row)
        {
            var currentRowScore = 0;

            // start with col 0
            var currentRunLength = 1;
            var currentRunIsDark = this._modules.IsDark(row, 0);

            for (var col = 1; col < this.Width; col++)
            {
                var currentModuleIsDark = this._modules.IsDark(row, col);
                if (currentModuleIsDark == currentRunIsDark)
                {
                    currentRunLength++;
                }
                else
                {
                    if (currentRunLength >= 5)
                    {
                        currentRowScore += currentRunLength - 2;
                    }
                    currentRunLength = 1;
                    currentRunIsDark = currentModuleIsDark;
                }
            }

            if (currentRunLength >= 5)
            {
                currentRowScore += currentRunLength - 2;
            }
            return currentRowScore;
        }

        private int CalculatePenaltyScoreA1ForCol(int col)
        {
            var currentColScore = 0;

            // start with row 0
            var currentRunLength = 1;
            var currentRunIsDark = this._modules.IsDark(0, col);

            for (var row = 1; row < this.Width; row++)
            {
                var currentModuleIsDark = this._modules.IsDark(row, col);
                if (currentModuleIsDark == currentRunIsDark)
                {
                    currentRunLength++;
                }
                else
                {
                    if (currentRunLength >= 5)
                    {
                        currentColScore += currentRunLength - 2;
                    }
                    currentRunLength = 1;
                    currentRunIsDark = currentModuleIsDark;
                }
            }

            if (currentRunLength >= 5)
            {
                currentColScore += currentRunLength - 2;
            }
            return currentColScore;
        }

        // Block of modules in same color
        // Eval condition: block size = m * n
        // Points: 3 * (m - 1) * (n - 1)
        private int CalculatePenaltyScoreB()
        {
            // Let's simplify the calculation by using the fact that the number of 2x2 squares
            // that fit into a block of size m * n is (m - 1) * (n - 1). So we'll just count
            // the number of 2x2 squares and multiply by 3 (the weight per occurrence).

            var numOccurrences = 0;
            for (var row = 0; row < this.Width - 1; row++)
            for (var col = 0; col < this.Width - 1; col++)
            {
                var currentModuleIsDark = this._modules.IsDark(row, col);
                var is2X2Square = currentModuleIsDark == this._modules.IsDark(row, col + 1) &&
                                  currentModuleIsDark == this._modules.IsDark(row + 1, col) &&
                                  currentModuleIsDark == this._modules.IsDark(row + 1, col + 1);
                if (is2X2Square)
                {
                    numOccurrences++;
                }
            }

            return numOccurrences * 3;
        }

        // Unexpected finder pattern present in symbol
        // Eval condition: 1:1:3:1:1 ratio dark:light:dark:light:dark preceded or followed by light area 4 modules wide
        // Points: 40 points per occurrence
        private int CalculatePenaltyScoreC()
        {
            var numOccurrences = 0;

            for (var row = 0; row < this.Width; row++)
            {
                var detector = new FinderPatternDetector();
                for (var col = 0; col < this.Width; col++)
                {
                    var currentModuleIsDark = this._modules.IsDark(row, col);
                    detector.ShiftIn(currentModuleIsDark);
                }
                numOccurrences += detector.NumFinderPatternsFound;
            }

            for (var col = 0; col < this.Width; col++)
            {
                var detector = new FinderPatternDetector();
                for (var row = 0; row < this.Width; row++)
                {
                    var currentModuleIsDark = this._modules.IsDark(row, col);
                    detector.ShiftIn(currentModuleIsDark);
                }
                numOccurrences += detector.NumFinderPatternsFound;
            }

            return numOccurrences * 40;
        }

        private int CalculatePenaltyScoreD()
        {
            int totalLightModuleCount = 0,
                totalDarkModuleCount = 0;

            for (var row = 0; row < this.Width; row++)
            for (var col = 0; col < this.Width; col++)
            {
                var isDarkModule = this._modules.IsDark(row, col);
                if (isDarkModule)
                {
                    totalDarkModuleCount++;
                }
                else
                {
                    totalLightModuleCount++;
                }
            }

            var highestColorAsPercent = 100 * Math.Max(totalDarkModuleCount, totalLightModuleCount) / (totalDarkModuleCount + totalLightModuleCount);
            var step = (highestColorAsPercent - 50) / 5;
            return step * 10; // weight = 10
        }

        // Predicates given by ISO/IEC 18004:2006(E), Sec. 6.8.1, Table 10
        private delegate bool DataMaskPredicate(int row, int col);
    }
}
