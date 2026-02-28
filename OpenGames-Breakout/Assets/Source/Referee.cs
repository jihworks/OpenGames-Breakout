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
    public partial class Referee : System.IDisposable
    {
        public Player Player { get; }
        public Round Round { get; }

        public BallScript Ball { get; }
        public PaddleScript Paddle { get; }
        public Transform BlocksRoot { get; }
        public Transform BallGhostsRoot { get; }
        public InputSystem_Actions InputSystemActions { get; }

        public float KillY { get; }
        public float CurrentBallSpeed { get; private set; }
        public float CurrentPaddleSpeed { get; private set; }

        public Vector2 CurrentBallDirection { get; private set; }

        StateStorage<State> _state = new(nameof(Referee));
        State? CurrentState { get => _state.Current; set => _state.Current = value; }

        readonly List<BlockScript> _blocks = new();

        readonly List<BallGhostScript> _ballGhosts = new();

        readonly BallMarcher _ballMarcher = new();
        readonly Prophet _prophet = new();

        readonly RandomStream _random = new();

        public Referee(Player player, Round round)
        {
            Player = player;
            Round = round;

            _ballMarcher.Hit += BallMarcher_Hit;

            Main main = Main.Instance;
            Ball = main.Ball;
            Paddle = main.Paddle;
            BlocksRoot = main.BlocksRoot;
            BallGhostsRoot = main.BallGhostsRoot;
            InputSystemActions = main.InputSystemActions;

            Rules rules = Rules.Instance;
            KillY = rules.KillY;
            CurrentBallSpeed = rules.BaseBallSpeed;
            CurrentPaddleSpeed = rules.BasePaddleSpeed;

            CurrentState = new Sleep(this);
        }

        public void BeginPlay()
        {
            Rules rules = Rules.Instance;
            CurrentBallDirection = rules.GetBallInitialShootDirection(_random);

            CurrentState = new Play(this);
        }

        void PlaceBlocksForBeginPlay()
        {
            Assets assets = Assets.Instance;
            Rules rules = Rules.Instance;

            Vector2 blockSize = rules.BlockOccupyingSize;

            for (int r = 0; r < rules.BlockRowCount; r++)
            {
                float y = blockSize.y * r;

                GameObject blockPrefab = assets.GetBlockPrefab(r);

                for (int c = 0; c < rules.BlockColumnCount; c++)
                {
                    float x = blockSize.x * c;

                    if (!Round.IsInitialBlockExistsAt(r, c))
                    {
                        continue;
                    }

                    GameObject blockObject = Object.Instantiate(blockPrefab, BlocksRoot);
                    blockObject.transform.localPosition = new Vector3(x, y, 0f);

                    BlockScript block = blockObject.GetComponentOrThrow<BlockScript>();
                    _blocks.Add(block);

                    block.Initialize(r, c);
                }
            }

            foreach (var block in _blocks)
            {
                block.BeginPlay(this);
            }
        }

        public void Update()
        {
            CurrentState?.Update();
        }

        public void FixedUpdate()
        {
            CurrentState?.FixedUpdate();
        }

        void UpdatePaddleInput()
        {
            if (Time.timeScale <= 0f)
            {
                return;
            }

            float inputX = InputSystemActions.Player.Move.ReadValue<Vector2>().x;
            if (inputX.IsNearlyZero())
            {
                return;
            }

            Rules rules = Rules.Instance;

            PaddleScript paddle = Paddle;
            Vector2 location = paddle.transform.localPosition;

            float delta = CurrentPaddleSpeed * Time.fixedDeltaTime;

            location.x += delta * inputX;

            float limitX = rules.PaddleXLimit;
            float halfWidth = rules.PaddleTotalWidth / 2f;
            MathEx.Clamp(ref location.x, -limitX + halfWidth, limitX - halfWidth);

            paddle.transform.localPosition = location;

            Physics2D.SyncTransforms();
        }

        void UpdateBallMovement()
        {
            if (Time.timeScale <= 0f)
            {
                return;
            }

            if (CurrentBallDirection.IsNearlyZero())
            {
                return;
            }

            Rules rules = Rules.Instance;
            float radius = rules.BallRadius;

            Vector2 currentLocation = Ball.transform.position;

            float speed = CurrentBallSpeed;
            float distance = speed * Time.fixedDeltaTime;

            BallMarcher ballMarcher = _ballMarcher;
            ballMarcher.March(currentLocation, radius, CurrentBallDirection, distance);

            Ball.transform.position = ballMarcher.CurrentLocation;
            CurrentBallDirection = ballMarcher.CurrentDirection;
        }

        void UpdateGhosts()
        {
            if (CurrentBallDirection.IsNearlyZero())
            {
                HideGhosts();
                return;
            }

            Rules rules = Rules.Instance;

            Vector2 currentLocation = Ball.transform.position;

            _prophet.Predict(currentLocation, rules.BallRadius, CurrentBallDirection);
            PlaceGhosts();
        }

        private void BallMarcher_Hit(BallMarcher arg1, RaycastHit2D arg2)
        {
            Collider2D collider = arg2.collider;

            if (Main.IsBlock(collider, out BlockScript? block))
            {
                HandleHit(block, arg2);
            }
            //else if (Main.IsPaddle(collider, out PaddleScript? paddle))
            //{
            //}
            //else if (Main.IsWall(collider, out WallScript? wall))
            //{
            //}
        }

        public void HandleHit(BlockScript block, RaycastHit2D _2)
        {
            if (!_blocks.Remove(block))
            {
                Debug.LogWarning("Block not found in the list.");
                return;
            }

            block.EndPlay();
            Object.Destroy(block.gameObject);
        }

        void PlaceGhosts()
        {
            IReadOnlyList<Vector2> predictedHitLocations = _prophet.PredictedHitLocations;

            GameObject ballGhostPrefab = Assets.Instance.BallGhostPrefab;
            for (int i = _ballGhosts.Count; i < predictedHitLocations.Count; i++)
            {
                GameObject ghostBallObj = Object.Instantiate(ballGhostPrefab, BallGhostsRoot);
                BallGhostScript ballGhost = ghostBallObj.GetComponentOrThrow<BallGhostScript>();
                _ballGhosts.Add(ballGhost);
            }

            for (int i = 0; i < predictedHitLocations.Count; i++)
            {
                Vector2 location = predictedHitLocations[i];

                BallGhostScript ballGhost = _ballGhosts[i];
                ballGhost.transform.position = location;

                ballGhost.SetVisible(true);
            }
            for (int i = predictedHitLocations.Count; i < _ballGhosts.Count; i++)
            {
                BallGhostScript ballGhost = _ballGhosts[i];

                ballGhost.SetVisible(false);
            }
        }

        void HideGhosts()
        {
            foreach (BallGhostScript ballGhost in _ballGhosts)
            {
                ballGhost.SetVisible(false);
            }
        }

        public void Dispose()
        {
            foreach (BlockScript block in _blocks)
            {
                Object.Destroy(block.gameObject);
            }
            _blocks.Clear();

            foreach (BallGhostScript ballGhost in _ballGhosts)
            {
                Object.Destroy(ballGhost.gameObject);
            }
            _ballGhosts.Clear();
        }
    }
}
