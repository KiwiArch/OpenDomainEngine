namespace Ode.Domain.Engine.SampleModel.Locations
{
    using Ode.Domain.Engine.SampleModel.Locations.ValueObjects;

    public class AdjustIn
    {
        public AdjustIn(string adjustment, string location, StockItem item)
        {
            this.Adjustment = adjustment;
            this.Location = location;
            this.Item = item;
        }

        public string Location { get; private set; }

        public string Adjustment { get; private set; }

        public StockItem Item { get; private set; }
    }
}
