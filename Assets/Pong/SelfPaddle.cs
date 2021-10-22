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
    public class SelfPaddle : MonoBehaviour
    {
        private const float _PADDLE_SPEED_Y = .05f;

        private SelfVar<float> _paddleY = new SelfVar<float>();
        private KeyCode _paddleUpKey;
        private KeyCode _paddleDownKey;

        public void Init(VarRegistry varRegistry, KeyCode paddleUpKey, KeyCode paddleDownKey)
        {
            varRegistry.Register("paddleY", _paddleY);
            _paddleY.OnValueChanged += HandlePaddleYChanged;
            _paddleUpKey = paddleUpKey;
            _paddleDownKey = paddleDownKey;
        }

        private void MovePaddleUp()
        {
            float newY = _paddleY.GetValue() + _PADDLE_SPEED_Y;
            _paddleY.SetValue(newY);
        }

        private void MovePaddleDown()
        {
            float newY = _paddleY.GetValue() - _PADDLE_SPEED_Y;
            _paddleY.SetValue(newY);
        }

        private void HandlePaddleYChanged(ISelfVarEvent<float> e)
        {
            float x = transform.position.x;
            float z = transform.position.z;
            transform.position = new Vector3(x, e.ValueChange.NewValue, z);
        }

        private void Update()
        {
            if (Input.GetKey(_paddleDownKey))
            {
                MovePaddleDown();
            }
            else if (Input.GetKey(_paddleUpKey))
            {
                MovePaddleUp();
            }
        }
    }
}
