namespace Ode.Domain.Engine.SampleModel.Locations
{
    using Ode.Domain.Engine.SampleModel.Locations.ValueObjects;
    
    public class MoveIn : AdjustIn
    {
        public MoveIn(string movement, string location, StockItem item, string fromLocation)
            :base(movement, location, item)
        {
            this.Movement = movement;
            //this.Location = location;
            //this.Item = item;
            this.FromLocation = fromLocation;
        }

       // public string Location { get; private set; }

        public string Movement { get; private set; }

       // public StockItem Item { get; private set; }

        public string FromLocation { get; private set; }
    }
}
