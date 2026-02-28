// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using Jih.OpenGames.Runtime;

namespace Jih.OpenGames.Breakout
{
    public partial class Referee
    {
        abstract class State : StateBase<Referee>
        {
            protected State(Referee owner) : base(owner)
            {
            }

            public virtual void FixedUpdate()
            {
            }
        }

        class Sleep : State
        {
            public Sleep(Referee owner) : base(owner)
            {
            }
        }

        class Play : State
        {
            public Play(Referee owner) : base(owner)
            {
            }

            public override void Begin(IState? prev)
            {
                base.Begin(prev);

                Owner.PlaceBlocksForBeginPlay();
                Owner.Paddle.BeginPlay(Owner);
                Owner.Ball.BeginPlay(Owner);
            }

            public override void End(IState? next)
            {
                Owner.Ball.EndPlay();
                Owner.Paddle.EndPlay();
                foreach (var block in Owner._blocks)
                {
                    block.EndPlay();
                }

                base.End(next);
            }

            public override void Update()
            {
                base.Update();

                Owner.UpdateGhosts();
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();

                Owner.UpdatePaddleInput();
                Owner.UpdateBallMovement();
            }
        }
    }
}
