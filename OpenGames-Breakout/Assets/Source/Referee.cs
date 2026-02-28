// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

namespace Jih.OpenGames.Breakout
{
    public class Referee
    {
        public Player Player { get; }
        public Round Round { get; }

        public Referee(Player player, Round round)
        {
            Player = player;
            Round = round;
        }
    }
}
