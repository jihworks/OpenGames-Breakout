// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using System;
using UnityEngine;

namespace Jih.OpenGames.Breakout
{
    public class BallGhostScript : MonoBehaviour
    {
        [SerializeField] SpriteRenderer[] _spriteRenderers = Array.Empty<SpriteRenderer>();

        public void SetVisible(bool visible)
        {
            foreach (SpriteRenderer spriteRenderer in _spriteRenderers)
            {
                spriteRenderer.enabled = visible;
            }
        }
    }
}
