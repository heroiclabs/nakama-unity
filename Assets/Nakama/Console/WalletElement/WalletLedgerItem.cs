using System;
using UnityEngine;

namespace Nakama.Console
{
    [Serializable]
    internal class WalletLedgerItem 
    { 
        [SerializeField] private string id;
        [SerializeField] private float amount;

        internal string Id { get => id; set => id = value; }
        internal float Amount { get => amount; set => amount = value; }
    }
}
