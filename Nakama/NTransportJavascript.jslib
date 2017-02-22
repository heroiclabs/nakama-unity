var WebSocketTransport = {
    $callbackObject: {},
    $sockets: [],

    InitTransport: function(authSuccessCallback, authErrorCallback, onSocketOpen, onSocketError, onSocketMessage, onSocketClose)
    {
        callbackObject.authSuccessCallback = authSuccessCallback;
        callbackObject.authErrorCallback = authErrorCallback;

        callbackObject.onSocketOpen = onSocketOpen;
        callbackObject.onSocketError = onSocketError;
        callbackObject.onSocketMessage = onSocketMessage;
        callbackObject.onSocketClose = onSocketClose;
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

    CreateSocket: function(socketPtr, serverUriPtr)
    {
        var socketId = Pointer_stringify(socketPtr);
        var serverUri = Pointer_stringify(serverUriPtr);
        var socket = new WebSocket(serverUri);

        socket.onopen = function (e) {
            var socketIdBuffer = _malloc(lengthBytesUTF8(socketId) + 1);
            writeStringToMemory(socketId, socketIdBuffer);
            Runtime.dynCall('vi', callbackObject.onSocketOpen, [socketIdBuffer]);
            _free(socketIdBuffer);
        };
        socket.onerror = function (e) {
            var socketIdBuffer = _malloc(lengthBytesUTF8(socketId) + 1);
            writeStringToMemory(socketId, socketIdBuffer);
            Runtime.dynCall('vi', callbackObject.onSocketError, [socketIdBuffer]);
            _free(socketIdBuffer);

            socket.close();
        };
        socket.onmessage = function (e) {
            if (e.data instanceof Blob)
            {
                var reader = new FileReader();
                reader.addEventListener("loadend", function() {
                    var byteData = new Uint8Array(reader.result);
                    var base64Data = btoa(String.fromCharCode.apply(null, byteData));
                    var dataBuffer = _malloc(lengthBytesUTF8(base64Data) + 1);
                    writeStringToMemory(base64Data, dataBuffer);

                    var socketIdBuffer = _malloc(lengthBytesUTF8(socketId) + 1);
                    writeStringToMemory(socketId, socketIdBuffer);

                    Runtime.dynCall('vii', callbackObject.onSocketMessage, [socketIdBuffer, dataBuffer]);
                    _free(dataBuffer);
                    _free(socketIdBuffer);
                });
                reader.readAsArrayBuffer(e.data);
            }
        };
        socket.onclose = function (e) {
            var socketIdBuffer = _malloc(lengthBytesUTF8(socketId) + 1);
            writeStringToMemory(socketId, socketIdBuffer);

            var closeCode = '' + e.code;
            var closeCodeBuffer = _malloc(lengthBytesUTF8(closeCode) + 1);
            writeStringToMemory(closeCode, closeCodeBuffer);

            Runtime.dynCall('vii', callbackObject.onSocketClose, [socketIdBuffer, closeCodeBuffer]);
            _free(socketIdBuffer);
            _free(closeCodeBuffer);

            //TODO(mo) do we need to remove the socket reference from sockets array?
        };

        var instance = sockets.push(socket) - 1;
	    return instance;
    },

    SocketState: function(socketRef)
    {
        var socket = sockets[socketRef];
        return socket.readyState;
    },

    CloseSocket: function (socketRef)
    {
        var socket = sockets[socketRef];
        socket.close();
    },

    SendData: function (socketRef, payload)
    {
        var base64Payload = Pointer_stringify(payload);
        var binaryString = window.atob(base64Payload);
        var len = binaryString.length;
        var bytes = new Uint8Array(len);
        for (var i = 0; i < len; i++) {
            bytes[i] = binaryString.charCodeAt(i);
        }
        var payloadBytes = bytes.buffer;

        var socket = sockets[socketRef];
        socket.send(new Blob([payloadBytes], {type : 'application/octet-stream'}));
    }
};

autoAddDeps(WebSocketTransport, '$callbackObject');
autoAddDeps(WebSocketTransport, '$sockets');
mergeInto(LibraryManager.library, WebSocketTransport);
