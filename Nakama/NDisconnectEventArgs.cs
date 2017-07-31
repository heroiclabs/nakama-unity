using Nakama;
using System;

public class NDisconnectEventArgs : EventArgs
{
    public int Code { get; private set; }

    public string Reason { get; private set; }

    internal NDisconnectEventArgs(int code, string reason)
    {
        Code = code;
        Reason = reason;
    }
}
