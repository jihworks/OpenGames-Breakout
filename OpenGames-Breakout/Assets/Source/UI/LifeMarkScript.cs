// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using UnityEngine;
using UnityEngine.UI;

namespace Jih.OpenGames.Breakout.UI
{
    public class LifeMarkScript : MonoBehaviour
    {
        [SerializeField] Image? _mainImage;
        public Image MainImage => _mainImage.ThrowIfNull(nameof(MainImage));

        [SerializeField] Color _aliveColor = Color.white;
        public Color AliveColor => _aliveColor;

        [SerializeField] Color _deadColor = Color.gray;
        public Color DeadColor => _deadColor;

        public void SetVisible(bool visible)
        {
            MainImage.enabled = visible;
        }

        public void SetAlive(bool alive)
        {
            MainImage.color = alive ? AliveColor : DeadColor;
        }
    }
}
