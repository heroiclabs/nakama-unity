/**
* Copyright 2021 The Nakama Authors
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

using UnityEngine;
using NakamaSync;

namespace Pong
{
    public class OpponentPaddle : MonoBehaviour
    {
        private PresenceVar<float> _paddleY = new PresenceVar<float>();

        public void Init(VarRegistry varRegistry)
        {
            varRegistry.Register("paddleY", _paddleY);
            _paddleY.OnValueChanged += HandlePaddleYChanged;
        }

        private void HandlePaddleYChanged(IPresenceVarEvent<float> e)
        {
            Debug.Log("Opponent paddle y changed.");
            float x = transform.position.x;
            float z = transform.position.z;
            transform.position = new Vector3(x, e.ValueChange.NewValue, z);
        }
    }
}
