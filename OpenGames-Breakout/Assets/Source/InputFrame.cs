// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using Jih.OpenGames.Runtime;

namespace Jih.OpenGames.Breakout
{
    public class InputFrame : InputFrame<InputSystem_Actions>
    {
        public InputFrame(object holder, bool ui) : base(holder, ui)
        {
        }

        protected override void DoApply(InputSystem_Actions inputActions)
        {
        }
    }
}
