// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using UnityEngine;

namespace Jih.OpenGames.Breakout
{
    [RequireComponent(typeof(Collider2D))]
    public class ColliderScript : MonoBehaviour
    {
        [SerializeField] MonoBehaviour? _owner;
        public MonoBehaviour Owner => _owner.ThrowIfNull(nameof(Owner));

        Collider2D? _collider;
        public Collider2D Collder
        {
            get
            {
                if (_collider == null)
                {
                    _collider = GetComponent<Collider2D>();
                }
                return _collider;
            }
        }
    }
}
