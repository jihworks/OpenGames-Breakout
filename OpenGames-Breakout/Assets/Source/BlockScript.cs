// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using UnityEngine;

namespace Jih.OpenGames.Breakout
{
    public class BlockScript : MonoBehaviour
    {
        public int RowIndex { get; private set; }
        public int ColumnIndex { get; private set; }

        public Referee? Referee { get; private set; }

        public void Initialize(int rowIndex, int columnIndex)
        {
            RowIndex = rowIndex;
            ColumnIndex = columnIndex;
        }

        public void BeginPlay(Referee referee)
        {
            Referee = referee;
        }

        public void EndPlay()
        {
            Referee = null;
        }
    }
}
