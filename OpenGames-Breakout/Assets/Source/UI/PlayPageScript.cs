// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using Jih.OpenGames.UI;
using System;
using UnityEngine;

namespace Jih.OpenGames.Breakout.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PlayPageScript : MonoBehaviour, IUILayer
    {
        [SerializeField] LifeMarkScript[] _lifeMarks = Array.Empty<LifeMarkScript>();

        [SerializeField] GameObject? _startInfoRoot;
        GameObject StartInfoRoot => _startInfoRoot.ThrowIfNull(nameof(StartInfoRoot));

        CanvasGroup? _canvasGroup;
        CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                {
                    _canvasGroup = GetComponent<CanvasGroup>();
                }
                return _canvasGroup;
            }
        }

        void Awake()
        {
            gameObject.SetActive(false);
        }

        void Start()
        {
#if DEBUG
            {
                Rules rules = Rules.Instance;

                UnityEngine.Assertions.Assert.IsTrue(_lifeMarks.Length == rules.MaxPlayerLifeCount, "Life count mismatch.");
            }
#endif
        }

        public void SetStartInfo(bool visible)
        {
            StartInfoRoot.SetActive(visible);
        }

        public void SetMaxLife(int maxLife)
        {
            int index = 0;
            for (; index < maxLife; index++)
            {
                _lifeMarks[index].SetVisible(true);
            }
            for (; index < _lifeMarks.Length; index++)
            {
                _lifeMarks[index].SetVisible(false);
            }
        }

        public void SetLife(int life)
        {
            int index = 0;
            for (; index < life; index++)
            {
                _lifeMarks[index].SetAlive(true);
            }
            for (; index < _lifeMarks.Length; index++)
            {
                _lifeMarks[index].SetAlive(false);
            }
        }

        CanvasGroup IUILayer.GetRootCanvasGroup()
        {
            return CanvasGroup;
        }

        GameObject? IUILayer.GetFocusedObject()
        {
            return null;
        }

        void IUILayer.OnActivating()
        {
        }
        void IUILayer.OnActivated()
        {
        }

        void IUILayer.OnDeactivating()
        {
        }
        void IUILayer.OnDeactivated()
        {
        }

        void IUILayer.OnObjectFocusing()
        {
        }
        void IUILayer.OnObjectFocused(GameObject? focusedObject)
        {
        }

        void IUILayer.PerformAction(string id, object? args)
        {
        }

        void IUILayer.OnAttaching()
        {
            gameObject.SetActiveSelfIfDiff(true);
        }

        void IUILayer.OnAttached()
        {
        }

        void IUILayer.Dettach()
        {
            gameObject.SetActiveSelfIfDiff(false);
        }
    }
}
