// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System.Collections.Generic;
using UnityEngine;

namespace Jih.OpenGames.Breakout
{
    public class Prophet
    {
        public int MaxPredictionCount { get; set; } = 10;

        readonly List<Vector2> _predictedHitLocations = new();
        public IReadOnlyList<Vector2> PredictedHitLocations => _predictedHitLocations;

        readonly ProphetBallMarcher _ballMarcher;

        public Prophet()
        {
            _ballMarcher = new ProphetBallMarcher(this);
        }

        public void Predict(Vector2 startLocation, float ballRadius, Vector2 startDirection)
        {
            _predictedHitLocations.Clear();

            _ballMarcher.March(startLocation, ballRadius, startDirection, float.PositiveInfinity);
        }

        class ProphetBallMarcher : BallMarcher
        {
            public Prophet Owner { get; }

            public ProphetBallMarcher(Prophet owner)
            {
                Owner = owner;
            }

            protected override void OnHit(RaycastHit2D hit, ref bool stop)
            {
                base.OnHit(hit, ref stop);

                Owner._predictedHitLocations.Add(hit.centroid);
                if (Owner._predictedHitLocations.Count >= Owner.MaxPredictionCount)
                {
                    stop = true;
                }
                if (Main.IsBlock(hit.collider, out _))
                {
                    // If detected first block hit, stop prediction.
                    stop = true;
                }
            }
        }
    }
}
