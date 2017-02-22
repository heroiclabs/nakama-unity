var WebSocketTransport = {
    $callbackObject: {},

    InitTransport: function(authSuccessCallback, authErrorCallback)
    {
        callbackObject.authSuccessCallback = authSuccessCallback;
        callbackObject.authErrorCallback = authErrorCallback;
    },

    FetchPost: function(handler, uri, payload, authHeader, langHeader)
    {
        var handlerId = Pointer_stringify(handler);
        var base64Payload = Pointer_stringify(payload);

        var binaryString = window.atob(base64Payload);
        var len = binaryString.length;
        var bytes = new Uint8Array(len);
        for (var i = 0; i < len; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }
        var payloadBytes = bytes.buffer;

        var headers = new Headers({
            "Content-Type": "application/octet-stream;",
            "Accept": "application/octet-stream;",
            "Authorization": Pointer_stringify(authHeader),
            "Accept-Language": Pointer_stringify(langHeader),
            "Content-Length": len
        });

        var init = {
            method: 'POST',
            body: new Blob([payloadBytes], {type : 'application/octet-stream'}),
            headers: headers,
            mode: 'cors'
        };

        fetch(Pointer_stringify(uri), init).then(function(res) {
            var fileReader = new FileReader();
            fileReader.onload = function() {
                var byteData = new Uint8Array(this.result);
                var base64Data = btoa(String.fromCharCode.apply(null, byteData));
                var dataBuffer = _malloc(lengthBytesUTF8(base64Data) + 1);
                writeStringToMemory(base64Data, dataBuffer);

                var handlerBuffer = _malloc(lengthBytesUTF8(handlerId) + 1);
                writeStringToMemory(handlerId, handlerBuffer);

                Runtime.dynCall('vii', callbackObject.authSuccessCallback, [handlerBuffer, dataBuffer]);

                _free(dataBuffer);
                _free(handlerBuffer);
            };
            res.blob().then(function(myBlob) {
                fileReader.readAsArrayBuffer(myBlob);
            });
        }).catch(function(error) {
            var handlerBuffer = _malloc(lengthBytesUTF8(handlerId) + 1);
            writeStringToMemory(handlerId, handlerBuffer);
            Runtime.dynCall('vi', callbackObject.authErrorCallback, [handlerBuffer]);
            _free(handlerBuffer);
        });
    },

    CreateSocket: function(uri)
    {
        var str = Pointer_stringify(uri);
        socket: new WebSocket(str);

        socket.addEventListener('open', function (event) {
            console.log("connection opened");
            NTransportJavascript.OnSocketOpen();
        });

        socket.onmessage = function (e) {
            // Todo: handle other data types?
            if (e.data instanceof Blob)
            {
                var reader = new FileReader();
                reader.addEventListener("loadend", function() {
                    var array = new Uint8Array(reader.result);
                    socket.messages.push(array);
                });
                reader.readAsArrayBuffer(e.data);
            }
        };
        socket.onclose = function (e) {
            if (e.code != 1000)
            {
                if (e.reason != null && e.reason.length > 0)
                    socket.error = e.reason;
                else
                {
                    switch (e.code)
                    {
                        case 1001:
                            socket.error = "Endpoint going away.";
                            break;
                        case 1002:
                            socket.error = "Protocol error.";
                            break;
                        case 1003:
                            socket.error = "Unsupported message.";
                            break;
                        case 1005:
                            socket.error = "No status.";
                            break;
                        case 1006:
                            socket.error = "Abnormal disconnection.";
                            break;
                        case 1009:
                            socket.error = "Data frame too large.";
                            break;
                        default:
                            socket.error = "Error "+e.code;
                    }
                }
            }
        }
    },

    SocketState: function (socketInstance)
    {
        // var socket = webSocketInstances[socketInstance];
        // return socket.socket.readyState;
    },

    SocketError: function (socketInstance, ptr, bufsize)
    {
        // var socket = webSocketInstances[socketInstance];
        // if (socket.error == null)
        //     return 0;
        // var str = socket.error.slice(0, Math.max(0, bufsize - 1));
        // writeStringToMemory(str, ptr, false);
        // return 1;
    },

    SocketSend: function (socketInstance, ptr, length)
    {
        // var socket = webSocketInstances[socketInstance];
        // socket.socket.send (HEAPU8.buffer.slice(ptr, ptr+length));
    },

    SocketRecvLength: function(socketInstance)
    {
        // var socket = webSocketInstances[socketInstance];
        // if (socket.messages.length == 0)
        //     return 0;
        // return socket.messages[0].length;
    },

    SocketRecv: function (socketInstance, ptr, length)
    {
        // var socket = webSocketInstances[socketInstance];
        // if (socket.messages.length == 0)
        //     return 0;
        // if (socket.messages[0].length > length)
        //     return 0;
        // HEAPU8.set(socket.messages[0], ptr);
        // socket.messages = socket.messages.slice(1);
    },

    SocketClose: function (socketInstance)
    {
        // var socket = webSocketInstances[socketInstance];
        // socket.socket.close();
    }
};

autoAddDeps(WebSocketTransport, '$callbackObject');
mergeInto(LibraryManager.library, WebSocketTransport);
