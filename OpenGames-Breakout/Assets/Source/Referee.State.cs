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

            public virtual void InputPlayerStart()
            {
            }
            public virtual void InputPlayerCancel()
            {
            }
        }

        class Sleep : State
        {
            public Sleep(Referee owner) : base(owner)
            {
            }
        }

        class StandBy : State
        {
            public StandByReason Reason { get; }

            public StandBy(Referee owner, StandByReason reason) : base(owner)
            {
                Reason = reason;
            }

            public override void Begin(IState? prev)
            {
                base.Begin(prev);

                Owner.Paddle.SetToInitialLocation();
                Owner.Ball.SetToInitialLocation();
                Owner.CurrentBallDirection = Owner.GetNextInitialBallShootDirection();

                Owner.PlayPage.SetStartInfo(true);
            }

            public override void End(IState? next)
            {
                Owner.PlayPage.SetStartInfo(false);

                base.End(next);
            }

            public override void InputPlayerStart()
            {
                Owner.CurrentState = new Play(Owner);
            }
        }

        class Play : State
        {
            public Play(Referee owner) : base(owner)
            {
            }

            public override void Update()
            {
                base.Update();

                Owner.UpdateGhosts();
                Owner.CheckVictory();
                Owner.CheckKillY();
            }

            public override void FixedUpdate()
            {
                base.FixedUpdate();

                Owner.UpdatePaddleInput();
                Owner.UpdateBallMovement();
            }
        }

        class Victory : State
        {
            public Victory(Referee owner) : base(owner)
            {
            }
        }

        class Defeated : State
        {
            public Defeated(Referee owner) : base(owner)
            {
            }
        }
    }
}
