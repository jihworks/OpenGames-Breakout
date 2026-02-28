// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace Jih.OpenGames.Breakout
{
    /// <remarks>
    /// <para>
    /// If the ball is penetrating(collision distance is close to zero) an object,<br/>
    /// other object is a block, it is treated as a valid hit.<br/>
    /// <br/>
    /// It assumes that <b>the block will be destroyed immediately</b>.<br/>
    /// If the block is not destroyed immediately, multi-hit or infinite loop can occur.<br/>
    /// <br/>
    /// If not handle as like this, collisions are often inaccurate between the moving block and the ball.<br/>
    /// <br/>
    /// For reference, adjusting <c>fixedDeltaTime</c> did not make much difference for this situation.
    /// </para>
    /// <para>
    /// On the other hand, if the other object is not a block, it is not a valid hit and the ball passes through.<br/>
    /// Especially, this can be observed in the paddle.<br/>
    /// <br/>
    /// This situation will be treated as a normal case in game design perspective. Because, it can considered as player's mistake.<br/>
    /// <br/>
    /// And also, contrastively, if this collision treat as a valid hit,<br/>
    /// paddle and ball do not push each other and also nothing destroyed, so it becomes an infinite loop.
    /// </para>
    /// </remarks>
    public class BallMarcher
    {
        public event System.Action<BallMarcher, RaycastHit2D>? Hit;

        public Vector2 CurrentLocation { get; private set; }
        public Vector2 CurrentDirection { get; private set; }
        public float RemainDistance { get; private set; }

        readonly List<RaycastHit2D> _hitsBuffer = new();

        public void March(Vector2 location, float radius, Vector2 direction, float distance)
        {
            CurrentLocation = location;
            CurrentDirection = direction;
            RemainDistance = distance;

            ContactFilter2D contactFilter = ContactFilter2D.noFilter;

            while (RemainDistance > 0.0001f)
            {
                _hitsBuffer.Clear();
                int hitCount = Physics2D.CircleCast(CurrentLocation, radius, CurrentDirection, contactFilter, _hitsBuffer, RemainDistance);
                if (hitCount <= 0)
                {
                    CurrentLocation += CurrentDirection * RemainDistance;
                    RemainDistance = 0f;
                    return;
                }

                if (!GetValidHit(_hitsBuffer, CurrentDirection, out RaycastHit2D hit))
                {
                    // Terminate if there is no valid hit.
                    CurrentLocation += CurrentDirection * RemainDistance;
                    RemainDistance = 0f;
                    return;
                }

                CurrentLocation = hit.centroid;
                RemainDistance -= hit.distance;

                if (Main.IsPaddle(hit.collider, out PaddleScript? paddle) &&
                    Vector2.Dot(hit.normal, Vector2.up) > 0f)
                {
                    // If the ball hits the upper surface of the paddle, apply special reflect algorithm.
                    CurrentDirection = Rules.Instance.GetPaddleHitOutDirection(paddle.transform.position.x, hit.point.x);
                }
                else
                {
                    CurrentDirection = Vector2.Reflect(CurrentDirection, hit.normal);

                    // If the reflected direction is too close to horizontal, set it to downward direction.
                    if (CurrentDirection.y.IsNearlyZero(Rules.Instance.HorizontalBallDirectionY))
                    {
                        CurrentDirection = Vector2.down;
                    }
                }

                bool stop = false;
                OnHit(hit, ref stop);
                if (stop)
                {
                    break;
                }
            }
        }

        protected virtual void OnHit(RaycastHit2D hit, ref bool stop)
        {
            Hit?.Invoke(this, hit);
        }

        static bool GetValidHit(List<RaycastHit2D> hits, Vector2 currentDirection, out RaycastHit2D result)
        {
            hits.Sort((l, r) => l.distance.CompareTo(r.distance));

            for (int h = 0; h < hits.Count; h++)
            {
                RaycastHit2D hit = hits[h];

                if (hit.distance.IsNearlyZero())
                {
                    // If the distance is very close, it is likely that the ball has penetrated to the object.
                    if (Main.IsBlock(hit.collider, out _))
                    {
                        // If it is a block, it is treated as a valid hit.
                        // Refer to the remarks for details.
                        goto FOUND;
                    }
                    // If it is not a block, ignore it(pass-through).
                    continue;
                }

                float cos = Vector2.Dot(hit.normal, -currentDirection);
                if (cos <= 0f)
                {
                    // If it is a back face collision, ignore it(pass-through).
                    continue;
                }

            FOUND:
                result = hit;
                return true;
            }

            result = default;
            return false;
        }
    }
}
