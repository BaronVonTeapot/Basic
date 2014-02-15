using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Core.Input
{
    public enum LogicalPlayerIndex
    {
        One,
        Two,
        Three,
        Four
    }

    public static class LogicalPlayerState
    {
        private static readonly PlayerIndex[] mPlayerIndices = 
        {
            PlayerIndex.One,
            PlayerIndex.Two,
            PlayerIndex.Three,
            PlayerIndex.Four
        };

        public static PlayerIndex GetPlayerIndex(LogicalPlayerIndex inIndex)
        {
            return mPlayerIndices[(int)inIndex];
        }

        public static void SetPlayerIndex(LogicalPlayerIndex inLogicalIndex,
                                          PlayerIndex inPlayerIndex)
        {
            mPlayerIndices[(int)inLogicalIndex] = inPlayerIndex;
        }
    }
}
