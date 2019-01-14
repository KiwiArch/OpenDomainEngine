namespace Ode.Domain.Engine.SampleModel
{
    using InventoryItems;

    public class InventoryItem
    {
        public InventoryItemCreated When(CreateInventoryItem command)
        {
            var stateChange = new InventoryItemCreated(command.Item);

            this.Then(stateChange);

            return stateChange;
        }

        private void Then(InventoryItemCreated stateChange)
        {
        }
    }
}
