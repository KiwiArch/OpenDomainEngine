namespace Ode.Domain.Engine.SampleModel.InventoryItems
{
    public class CreateInventoryItem
    {
        public CreateInventoryItem(string item)
        {
            this.Item = item;
        }

        public string Item { get; private set; }
    }
}
