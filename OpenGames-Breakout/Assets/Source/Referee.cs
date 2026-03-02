// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using Jih.OpenGames.Breakout.UI;
using Jih.OpenGames.Runtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

        public PlayPageScript PlayPage { get; }

        public float BallRadius { get; }
        /// <summary>
        /// In game space.
        /// </summary>
        public float KillY { get; }
        /// <summary>
        /// In game space.
        /// </summary>
        public float PaddleXLimit { get; }
        public float PaddleHalfWidth { get; }
        public float CurrentBallSpeed { get; private set; }
        public float CurrentPaddleSpeed { get; private set; }

        /// <summary>
        /// In world space.
        /// </summary>
        public Vector2 CurrentBallDirection { get; private set; }

        public int MaxLifeCount { get; private set; }
        public int CurrentLifeCount { get; private set; }

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

            PlayPage = main.MainCanvas.PlayPage;

            // Have to desubscribe in Dispose()
            InputSystemActions.Player.Start.performed += InputPlayerStart;
            InputSystemActions.Player.Cancel.performed += InputPlayerCancel;

            Rules rules = Rules.Instance;
            BallRadius = rules.BallRadius;
            KillY = rules.KillY;
            PaddleXLimit = rules.PaddleXLimit;
            PaddleHalfWidth = rules.PaddleTotalWidth * 0.5f;
            CurrentBallSpeed = rules.BaseBallSpeed;
            CurrentPaddleSpeed = rules.BasePaddleSpeed;

            MaxLifeCount = CurrentLifeCount = rules.MaxPlayerLifeCount;

            CurrentState = new Sleep(this);
        }

        public void BeginPlay()
        {
            PlayPage.SetMaxLife(MaxLifeCount);
            PlayPage.SetLife(CurrentLifeCount);

            PlaceBlocksForBeginPlay();
            Paddle.BeginPlay(this);
            Ball.BeginPlay(this);

            CurrentState = new StandBy(this, StandByReason.BeginPlay);
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

            PaddleScript paddle = Paddle;
            // In game space. The paddle is child of 'Game Root', so localPosition is used.
            Vector2 location = paddle.transform.localPosition;

            float delta = CurrentPaddleSpeed * Time.fixedDeltaTime;

            location.x += delta * inputX;

            MathEx.Clamp(ref location.x, -PaddleXLimit + PaddleHalfWidth, PaddleXLimit - PaddleHalfWidth);

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

            Vector2 currentLocation = Ball.transform.position;

            float speed = CurrentBallSpeed;
            float distance = speed * Time.fixedDeltaTime;

            BallMarcher ballMarcher = _ballMarcher;
            ballMarcher.March(currentLocation, BallRadius, CurrentBallDirection, distance);

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

            Vector2 currentLocation = Ball.transform.position;

            _prophet.Predict(currentLocation, BallRadius, CurrentBallDirection);
            PlaceGhosts();
        }

        void CheckVictory()
        {
            if (_blocks.Count > 0)
            {
                return;
            }

            CurrentState = new Victory(this);
        }

        void CheckKillY()
        {
            // In game space.
            // The Ball is child of 'Game Root', so localPosition is used.
            if (Ball.transform.localPosition.y > KillY)
            {
                return;
            }

            CurrentLifeCount--;
            PlayPage.SetLife(CurrentLifeCount);

            if (CurrentLifeCount <= 0)
            {
                CurrentState = new Defeated(this);
            }
            else
            {
                CurrentState = new StandBy(this, StandByReason.LoseLife);
            }
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

        Vector2 GetNextInitialBallShootDirection()
        {
            return Rules.Instance.GetBallInitialShootDirection(_random);
        }

        #region Input Handlers
        public void InputPlayerStart(InputAction.CallbackContext ctx)
        {
            CurrentState?.InputPlayerStart();
        }
        public void InputPlayerCancel(InputAction.CallbackContext ctx)
        {
            CurrentState?.InputPlayerCancel();
        }
        #endregion

        public void Dispose()
        {
            Ball.EndPlay();
            Paddle.EndPlay();
            foreach (var block in _blocks)
            {
                block.EndPlay();
            }

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

            InputSystemActions.Player.Start.performed -= InputPlayerStart;
            InputSystemActions.Player.Cancel.performed -= InputPlayerCancel;
        }

        enum StandByReason
        {
            BeginPlay,
            LoseLife,
        }
    }
}
