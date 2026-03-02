// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using Jih.OpenGames.Breakout.UI;
using Jih.OpenGames.Runtime;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.InputSystem.UI;

namespace Jih.OpenGames.Breakout
{
    public partial class Main : MonoBehaviour
    {
        static SingletonStorage<Main> _instance;
        public static Main Instance => _instance.Get();

        [SerializeField] InputSystemUIInputModule? _inputSystemUIInputModule;
        InputSystemUIInputModule InputSystemUIInputModule => _inputSystemUIInputModule.ThrowIfNull(nameof(InputSystemUIInputModule));

        [Header("Game Board")]
        [SerializeField] BallScript? _ball;
        public BallScript Ball => _ball.ThrowIfNull(nameof(Ball));

        [SerializeField] PaddleScript? _paddle;
        public PaddleScript Paddle => _paddle.ThrowIfNull(nameof(Paddle));

        [SerializeField] Transform? _blocksRoot;
        public Transform BlocksRoot => _blocksRoot.ThrowIfNull(nameof(BlocksRoot));

        [SerializeField] Transform? _ballGhostsRoot;
        public Transform BallGhostsRoot => _ballGhostsRoot.ThrowIfNull(nameof(BallGhostsRoot));

        [Header("UI")]
        [SerializeField] MainCanvas? _mainCanvas;
        public MainCanvas MainCanvas => _mainCanvas.ThrowIfNull(nameof(MainCanvas));

        public Player Player { get; } = new();

        InputSystem_Actions? _inputSystemActions;
        public InputSystem_Actions InputSystemActions => _inputSystemActions.ThrowIfNull(nameof(InputSystemActions));

        InputFrameStack<InputSystem_Actions>? _inputFrameStack;
        public InputFrameStack<InputSystem_Actions> InputFrameStack => _inputFrameStack.ThrowIfNull(nameof(InputFrameStack));

        CursorFrameStack? _cursorFrameStack;
        public CursorFrameStack CursorFrameStack => _cursorFrameStack.ThrowIfNull(nameof(CursorFrameStack));

        TimeFrameStack? _timeFrameStack;
        public TimeFrameStack TimeFrameStack => _timeFrameStack.ThrowIfNull(nameof(TimeFrameStack));

        StateStorage<State> _state = new(nameof(Main));
        State? CurrentState { get => _state.Current; set => _state.Current = value; }

        public Main()
        {
            _instance = new SingletonStorage<Main>(this);

            CurrentState = new Sleep(this);
        }

        void Awake()
        {
            _inputSystemActions = new InputSystem_Actions();
            _inputSystemActions.Enable();

            _inputSystemUIInputModule.ThrowIfNull(out InputSystemUIInputModule inputSystemUIInputModule, nameof(_inputSystemUIInputModule));
            inputSystemUIInputModule.actionsAsset = _inputSystemActions.asset;

            _inputFrameStack = new InputFrameStack<InputSystem_Actions>(_inputSystemActions, inputSystemUIInputModule);
            _cursorFrameStack = new CursorFrameStack();
            _timeFrameStack = new TimeFrameStack();
        }

        void Start()
        {
            // Clean up blocks placed in the editor.
            foreach (var obj in BlocksRoot.gameObject.EnumerateChildrenGameObjects())
            {
                Destroy(obj);
            }

            // Default runtime frames.
            InputFrameStack.Push(new InputFrame(this, ui: false, player: false));
            CursorFrameStack.Push(new CursorFrame(this, lockMode: CursorLockMode.None, cursorVisible: true));
            TimeFrameStack.Push(new TimeFrame(this, timeScale: 1f));

            Round round = new();
            CurrentState = new Play(this, Player, round);
        }

        void Update()
        {
            CurrentState?.Update();
        }

        void FixedUpdate()
        {
            CurrentState?.FixedUpdate();
        }

        public static bool IsPaddle(Collider2D collider, [NotNullWhen(true)] out PaddleScript? paddle)
        {
            ColliderScript colliderScript = collider.gameObject.GetComponentOrThrow<ColliderScript>();
            if (colliderScript.Owner is PaddleScript paddleScript)
            {
                paddle = paddleScript;
                return true;
            }
            paddle = null;
            return false;
        }
        public static bool IsBlock(Collider2D collider, [NotNullWhen(true)] out BlockScript? block)
        {
            ColliderScript colliderScript = collider.gameObject.GetComponentOrThrow<ColliderScript>();
            if (colliderScript.Owner is BlockScript blockScript)
            {
                block = blockScript;
                return true;
            }
            block = null;
            return false;
        }
        public static bool IsWall(Collider2D collider, [NotNullWhen(true)] out WallScript? wall)
        {
            ColliderScript colliderScript = collider.gameObject.GetComponentOrThrow<ColliderScript>();
            if (colliderScript.Owner is WallScript wallScript)
            {
                wall = wallScript;
                return true;
            }
            wall = null;
            return false;
        }
    }
}
