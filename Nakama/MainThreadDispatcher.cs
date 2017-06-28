/**
 * Copyright 2017 The Nakama Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Nakama
{
    // NOTE: Not compiled by default; avoids dependency on UnityEngine.
    // NOTE: This code is inspired by github:PimDeWitte/UnityMainThreadDispatcher.

    /// <summary>
    ///  A helper class which holds a queue with actions to dispatch on the next
    ///  Update() call. It can be used to make calls on the Unity main thread
    ///  which is necessary to interact with most Unity APIs.
    /// </summary>
    public class MainThreadDispatcher : MonoBehaviour
    {
        private static readonly Queue<IEnumerator> _executionQueue = new Queue<IEnumerator>(1024);

        private static MainThreadDispatcher _instance;

        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                _instance = this;
            }
        }

        private void Update()
        {
            lock(_executionQueue)
            {
                for (int i = 0, l = _executionQueue.Count; i < l; i++)
                {
                    StartCoroutine(_executionQueue.Dequeue());
                }
            }
        }

        IEnumerator ActionWrapper(Action action)
        {
            action();
            yield return null;
        }

        [PostProcessScene]
        private static void AddDispatcherToScene()
        {
            var dispatcherContainer = new GameObject("MainThreadDispatcher");
            DontDestroyOnLoad(dispatcherContainer);
            dispatcherContainer.AddComponent<MainThreadDispatcher>();
        }

        public static void Enqueue(IEnumerator action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }

        public static void Enqueue(Action action)
        {
            lock (_executionQueue)
            {
                _executionQueue.Enqueue(_instance.ActionWrapper(action));
            }
        }
    }
}
