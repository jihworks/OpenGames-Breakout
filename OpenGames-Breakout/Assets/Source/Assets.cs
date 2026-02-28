// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using Jih.OpenGames.Runtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Jih.OpenGames.Breakout
{
    public class Assets : MonoBehaviour
    {
        static SingletonStorage<Assets> _instance;
        public static Assets Instance => _instance.Get();

        [SerializeField] GameObject[] _blockAssets = Array.Empty<GameObject>();
        public IReadOnlyList<GameObject> BlockAssets => _blockAssets;

        [SerializeField] GameObject? _ballGhostAsset;
        public GameObject BallGhostAsset => _ballGhostAsset.ThrowIfNull(nameof(BallGhostAsset));

        public Assets()
        {
            _instance = new SingletonStorage<Assets>(this);
        }
    }
}
