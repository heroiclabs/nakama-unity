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
using System;

namespace Pong
{
    public class Ball : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D _rigidbody2D;

        private readonly SharedVar<float> _xPosition = new SharedVar<float>();
        private readonly SharedVar<float> _yPosition = new SharedVar<float>();

        public void Init(VarRegistry registry)
        {
            //registry.Register("ballX", _xPosition);
            //registry.Register("ballY", _yPosition);
            //_xPosition.OnValueChanged += HandleXChanged;
            //_yPosition.OnValueChanged += HandleYChanged;
        }

        private void HandleXChanged(ISharedVarEvent<float> obj)
        {
            this.transform.position = new Vector3(obj.ValueChange.NewValue, this.transform.position.y, this.transform.position.z);
        }

        private void HandleYChanged(ISharedVarEvent<float> obj)
        {
            this.transform.position = new Vector3(this.transform.position.x, obj.ValueChange.NewValue, this.transform.position.z);
        }

        private void Update()
        {
            _xPosition.SetValue(transform.position.x);
            _yPosition.SetValue(transform.position.y);
        }

        public void SetStartVelocity()
        {
            _rigidbody2D.velocity = new Vector3(3, 0, 0);
        }
    }
}