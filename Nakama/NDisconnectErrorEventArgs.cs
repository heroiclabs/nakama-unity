using Nakama;
using System;

public class NDisconnectErrorEventArgs : EventArgs
{
    public INError Error { get; private set; }

    internal NDisconnectErrorEventArgs(NError error)
    {
        Error = error;
    }
}
