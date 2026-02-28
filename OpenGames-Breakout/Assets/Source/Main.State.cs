// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using Jih.OpenGames.Runtime;

namespace Jih.OpenGames.Breakout
{
    public partial class Main
    {
        abstract class State : StateBase<Main>
        {
            public State(Main owner) : base(owner)
            {
            }
        }

        class Sleep : State
        {
            public Sleep(Main owner) : base(owner)
            {
            }
        }
    }
}
