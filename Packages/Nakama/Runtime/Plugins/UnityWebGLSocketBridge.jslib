/**
 * Copyright 2020 The Nakama Authors
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

var UnityWebGLSocketBridge = {
    $BRIDGE_NAME: {},
    $OPEN_METHOD_NAME: {},
    $CLOSE_METHOD_NAME: {},
    $MESSAGE_METHOD_NAME: {},
    $ERROR_METHOD_NAME: {},
    $SOCKET_REF_MAP: {},

    $INITIALIZE: function() {
        SOCKET_REF_MAP = new Map();
        BRIDGE_NAME = "[Nakama]";
        OPEN_METHOD_NAME = "NKBridgeOnOpen";
        CLOSE_METHOD_NAME = "NKBridgeOnClose";
        MESSAGE_METHOD_NAME = "NKBridgeOnMessage";
        ERROR_METHOD_NAME = "NKBridgeOnError";
    },

    NKCreateSocket: function(socketRef, addressPtr) {
        if (!(SOCKET_REF_MAP instanceof Map)) {
            INITIALIZE();
        }

        if (SOCKET_REF_MAP.has(socketRef)) {
            SendMessage(BRIDGE_NAME, ERROR_METHOD_NAME, socketRef + "_" + "A WebSocket already exists for this reference.");
            return;
        }

        var address = Pointer_stringify(addressPtr);
        var ws = new WebSocket(address);
        ws.onmessage = function(e) {
            if (typeof e.data == 'string') {
                SendMessage(BRIDGE_NAME, MESSAGE_METHOD_NAME, socketRef + "_" + e.data);
            } else {
                SendMessage(BRIDGE_NAME, ERROR_METHOD_NAME, socketRef + "_" + "Received invalid data from remote.");
            }
          };
        ws.onopen = function(e) {
            SendMessage(BRIDGE_NAME, OPEN_METHOD_NAME, socketRef + "_");
        };
        ws.onclose = function(e) {
            SOCKET_REF_MAP.delete(socketRef);
            SendMessage(BRIDGE_NAME, CLOSE_METHOD_NAME, socketRef + "_" + e.code);
        };
        ws.onerror = function(e) {
            // https://stackoverflow.com/questions/18803971/websocket-onerror-how-to-read-error-description
            SOCKET_REF_MAP.delete(socketRef);
            SendMessage(BRIDGE_NAME, ERROR_METHOD_NAME, socketRef + "_" + "WebSocket error: " + e.reason);
        };
        SOCKET_REF_MAP.set(socketRef, ws);
    },

    NKSocketState: function (socketRef) {
        if (!(SOCKET_REF_MAP instanceof Map)) {
            INITIALIZE();
        }

        if(!SOCKET_REF_MAP.has(socketRef)) {
            SendMessage(BRIDGE_NAME, ERROR_METHOD_NAME, socketRef + "_" + "Did not find websocket with given reference.");
            return -1;
        }
        return SOCKET_REF_MAP.get(socketRef).readyState;
    },

    NKCloseSocket: function (socketRef) {
        if (!(SOCKET_REF_MAP instanceof Map)) {
            INITIALIZE();
        }

        if(!SOCKET_REF_MAP.has(socketRef)) {
            SendMessage(BRIDGE_NAME, ERROR_METHOD_NAME, socketRef + "_" + "Did not find websocket with given reference.");
        } else {
            SOCKET_REF_MAP.get(socketRef).close();
        }
    },

    NKSendData: function (socketRef, msg) {
        if (!(SOCKET_REF_MAP instanceof Map)) {
            INITIALIZE();
        }

        if(!SOCKET_REF_MAP.has(socketRef)) {
            SendMessage(BRIDGE_NAME, ERROR_METHOD_NAME, socketRef + "_" + "Did not find websocket with given reference.");
        } else {
            var data = Pointer_stringify(msg);
            SOCKET_REF_MAP.get(socketRef).send(data);
        }
    },
};

// Auto add to depends
autoAddDeps(UnityWebGLSocketBridge, '$INITIALIZE');
autoAddDeps(UnityWebGLSocketBridge, '$SOCKET_REF_MAP');
autoAddDeps(UnityWebGLSocketBridge, '$BRIDGE_NAME');
autoAddDeps(UnityWebGLSocketBridge, '$OPEN_METHOD_NAME');
autoAddDeps(UnityWebGLSocketBridge, '$CLOSE_METHOD_NAME');
autoAddDeps(UnityWebGLSocketBridge, '$MESSAGE_METHOD_NAME');
autoAddDeps(UnityWebGLSocketBridge, '$ERROR_METHOD_NAME');
mergeInto(LibraryManager.library, UnityWebGLSocketBridge);
