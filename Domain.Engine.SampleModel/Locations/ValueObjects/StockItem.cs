namespace Ode.Domain.Engine.SampleModel.Locations.ValueObjects
{
    public class StockItem
    {
        public StockItem(string inventoryItem, string serial)
        {
            this.InventoryItem = inventoryItem;
            this.Serial = serial;
        }

        public string InventoryItem { get; private set; }

        public string Serial { get; private set; }
    }
}
