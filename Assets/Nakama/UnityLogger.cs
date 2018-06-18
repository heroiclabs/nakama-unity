/**
 * Copyright 2018 The Nakama Authors
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

namespace Nakama
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// A logger which writes to Unity's editor console.
    /// </summary>
    public sealed class UnityLogger : ILogger
    {
        public static readonly ILogger Instance = new UnityLogger();

        /// <inheritdoc />
        public void Debug(object message)
        {
            UnityEngine.Debug.Log(message);
        }

        /// <inheritdoc />
        public void DebugFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(format, args);
        }

        /// <inheritdoc />
        public void Info(object message)
        {
            UnityEngine.Debug.Log(message);
        }

        /// <inheritdoc />
        public void InfoFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogFormat(format, args);
        }

        /// <inheritdoc />
        public void Warn(object message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        /// <inheritdoc />
        public void WarnFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogWarningFormat(format, args);
        }

        /// <inheritdoc />
        public void Error(object message)
        {
            UnityEngine.Debug.LogError(message);
        }

        /// <inheritdoc />
        public void ErrorFormat(string format, params object[] args)
        {
            UnityEngine.Debug.LogErrorFormat(format, args);
        }
    }
}
