// © 2026 Jong-il Hong
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
// SPDX-License-Identifier: MIT

#nullable enable

using UnityEngine;

namespace Jih.OpenGames.Breakout
{
    public class PaddleScript : MonoBehaviour
    {
        public Referee? Referee { get; private set; }

        Vector3 _initialLocalLocation;

        void Awake()
        {
            _initialLocalLocation = transform.localPosition;
        }

        public void BeginPlay(Referee referee)
        {
            Referee = referee;

            transform.localPosition = _initialLocalLocation;
        }

        public void EndPlay()
        {
            Referee = null;
        }
    }
}
