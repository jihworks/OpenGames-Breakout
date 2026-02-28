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

        [SerializeField] GameObject[] _blockPrefabs = Array.Empty<GameObject>();
        public IReadOnlyList<GameObject> BlockPrefabs => _blockPrefabs;

        [SerializeField] GameObject? _ballGhostPrefab;
        public GameObject BallGhostPrefab => _ballGhostPrefab.ThrowIfNull(nameof(BallGhostPrefab));

        public Assets()
        {
            _instance = new SingletonStorage<Assets>(this);
        }

        public GameObject GetBlockPrefab(int rowIndex)
        {
            int prefabIndex = rowIndex / 2;
            return _blockPrefabs[prefabIndex];
        }
    }
}
