// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

namespace Jih.OpenGames.Runtime
{
    public abstract class StateBase<T> : IState
    {
        public T Owner { get; }

        public StateBase(T owner)
        {
            Owner = owner;
        }

        public virtual void Begin(IState? prev)
        {
        }
        public virtual void End(IState? next)
        {
        }

        public virtual void Update()
        {
        }
    }
}
