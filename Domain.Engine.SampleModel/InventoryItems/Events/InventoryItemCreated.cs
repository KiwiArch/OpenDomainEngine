namespace Ode.Domain.Engine.SampleModel.InventoryItems
{
    public class InventoryItemCreated
    {
        public InventoryItemCreated(string item)
        {
            this.Item = item;
        }

        public string Item { get; private set; }
    }
}
