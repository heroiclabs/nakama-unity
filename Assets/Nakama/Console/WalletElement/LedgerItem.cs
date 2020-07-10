namespace NakamaConsole
{
    internal class LedgerItem
    {
        public string ItemId { get; }
        public int Quantity { get; }

        public LedgerItem(string itemId, int quantity)
        {
            this.ItemId = itemId;
            this.Quantity = quantity;
        }
    }
}