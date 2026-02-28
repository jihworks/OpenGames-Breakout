// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

namespace Jih.OpenGames.Breakout
{
    public class Round
    {
        public bool IsInitialBlockExistsAt(int _0/*rowIndex*/, int _1/*columnIndex*/)
        {
            // Always place block at every possible locations.
            return true;
        }
    }
}
