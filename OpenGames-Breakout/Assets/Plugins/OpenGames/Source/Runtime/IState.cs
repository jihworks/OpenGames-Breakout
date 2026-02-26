// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

namespace Jih.OpenGames.Runtime
{
    public interface IState
    {
        void Begin(IState? prev);
        void End(IState? next);
    }
}
