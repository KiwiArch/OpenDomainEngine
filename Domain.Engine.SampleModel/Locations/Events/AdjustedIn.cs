namespace Ode.Domain.Engine.SampleModel.Locations
{
    using Ode.Domain.Engine.SampleModel.Locations.ValueObjects;

    public class AdjustedIn
    {
        public AdjustedIn(string adjustment, string location, StockItem item)
        {
            this.Adjustment = adjustment;
            this.Location = location;
            this.Item = item;
        }

        public string Adjustment { get; private set; }

        public string Location { get; private set; }

        public StockItem Item { get; private set; }
    }
}
