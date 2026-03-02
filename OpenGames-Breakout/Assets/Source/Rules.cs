// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using Jih.OpenGames.Runtime;
using System.Collections.Generic;
using UnityEngine;

namespace Jih.OpenGames.Breakout
{
    public class Rules : MonoBehaviour
    {
        static SingletonStorage<Rules> _instance;
        public static Rules Instance => _instance.Get();

        [Header("Ball")]
        [SerializeField] float _ballRadius = 20f;
        public float BallRadius => _ballRadius;

        [SerializeField, Tooltip("In game space. Check 'Game Root' in the main scene.")] float _killY = -720f;
        public float KillY => _killY;

        [SerializeField] float _baseBallSpeed = 500f;
        public float BaseBallSpeed => _baseBallSpeed;

        [SerializeField] float[] _initialBallShootDegrees = new float[]
        {
            30f, 45f, 60f, 120f, 135f, 150f,
        };
        public IReadOnlyList<float> InitialBallShootDegrees => _initialBallShootDegrees;

        [SerializeField] float _horizontalBallDirectionY = 0.05f; // ~= sin(3º)
        /// <summary>
        /// If reflected angle is less or equal than this value, it will be assume as horizontal.
        /// </summary>
        /// <seealso cref="BallMarcher"/>
        public float HorizontalBallDirectionY => _horizontalBallDirectionY;

        [Header("Paddle")]
        [SerializeField, Tooltip("Width of left or right wing's width. Not sum of both wing's width.")]
        private float _paddleWingWidth = 50f;
        public float PaddleWingWidth => _paddleWingWidth;
        [SerializeField, Tooltip("Sum width of left wing, right wing and body. It assuming there is no overlap.")]
        private float _paddleTotalWidth = 240f;
        public float PaddleTotalWidth => _paddleTotalWidth;

        [SerializeField] float _basePaddleSpeed = 500f;
        public float BasePaddleSpeed => _basePaddleSpeed;

        [SerializeField, Tooltip("In game space. Half range. Exclude half width of paddle.")] float _paddleXLimit = 600f;
        public float PaddleXLimit => _paddleXLimit;

        [SerializeField] float _paddleBodyHitAngleRange = 30f;
        public float PaddleBodyHitAngleRange => _paddleBodyHitAngleRange;

        [SerializeField] float _paddleWingHitStartAngle = 60f;
        public float PaddleWingHitStartAngle => _paddleWingHitStartAngle;
        [SerializeField] float _paddleWingHitAngleRange = 20f;
        public float PaddleWingHitAngleRange => _paddleWingHitAngleRange;

        [Header("Block")]
        [SerializeField] int _blockColumnCount = 6;
        public int BlockColumnCount => _blockColumnCount;
        [SerializeField] int _blockRowCount = 8;
        public int BlockRowCount => _blockRowCount;

        [SerializeField] Vector2 _blockOccupyingSize = new(200f, 100f);
        public Vector2 BlockOccupyingSize => _blockOccupyingSize;

        [Header("Player")]
        [SerializeField] int _maxPlayerLifeCount = 5;
        public int MaxPlayerLifeCount => _maxPlayerLifeCount;

        public Rules()
        {
            _instance = new SingletonStorage<Rules>(this);
        }

        /// <seealso cref="BallMarcher"/>
        public Vector2 GetPaddleHitOutDirection(float paddleX, float hitX)
        {
            float angle = GetPaddleHitOutAngle(paddleX, hitX);
            return MathEx.RadiusVector(angle.ToRadians());
        }
        /// <seealso cref="BallMarcher"/>
        public float GetPaddleHitOutAngle(float paddleX, float hitX)
        {
            float halfPaddleWidth = PaddleTotalWidth * 0.5f;
            float halfBodyWidth = halfPaddleWidth - PaddleWingWidth;

            float delta = hitX - paddleX;
            bool isLeft = delta < 0f;

            delta = Mathf.Abs(delta);

            float angle;
            if (delta <= halfBodyWidth)
            {
                // If hit on the body, the angle is mild.

                float alpha = delta / halfBodyWidth;
                if (isLeft)
                {
                    angle = Mathf.Lerp(90f, 90f + PaddleBodyHitAngleRange, alpha);
                }
                else
                {
                    angle = Mathf.Lerp(90f, 90f - PaddleBodyHitAngleRange, alpha);
                }
            }
            else
            {
                // If hit on the wing, the angle is steep.

                float alpha = (delta - halfBodyWidth) / PaddleWingWidth;
                if (isLeft)
                {
                    float startAngle = 90f + PaddleWingHitStartAngle;
                    angle = Mathf.Lerp(startAngle, startAngle + PaddleWingHitAngleRange, alpha);
                }
                else
                {
                    float startAngle = 90f - PaddleWingHitStartAngle;
                    angle = Mathf.Lerp(startAngle, startAngle - PaddleWingHitAngleRange, alpha);
                }
            }

            return angle;
        }

        public Vector2 GetBallInitialShootDirection(IRandomInt32 random)
        {
            int angleIndex = random.NextInt32(0, InitialBallShootDegrees.Count);
            float angle = InitialBallShootDegrees[angleIndex];

            return MathEx.RadiusVector(angle.ToRadians());
        }
    }
}
