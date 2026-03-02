// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using Jih.OpenGames.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Jih.OpenGames.Breakout.UI
{
    [RequireComponent(typeof(Canvas))]
    public class MainCanvas : MonoBehaviour
    {
        [SerializeField] EventSystem? _eventSystem;
        EventSystem EventSystem => _eventSystem.ThrowIfNull(nameof(EventSystem));

        [SerializeField] PlayPageScript? _playPage;
        public PlayPageScript PlayPage => _playPage.ThrowIfNull(nameof(PlayPage));

        UILayerStack? _uiLayerStack;
        UILayerStack UILayerStack => _uiLayerStack.ThrowIfNull(nameof(UILayerStack));

        void Awake()
        {
            _uiLayerStack = new UILayerStack(transform, EventSystem);
        }

        void Start()
        {
            UILayerStack.SetBottomLeast(PlayPage);
        }
    }
}
