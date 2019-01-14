namespace Ode.Domain.Engine.SampleModel.Locations
{
    using Ode.Domain.Engine.SampleModel.Locations.ValueObjects;
    
    public class MoveOut
    {
        public MoveOut(string movement, string location, StockItem item, string toLocation)
        {
            this.Movement = movement;
            this.Location = location;
            this.Item = item;
            this.ToLocation = toLocation;
        }

        public string Location { get; private set; }

        public string Movement { get; private set; }
        
        public StockItem Item { get; private set; }

        public string ToLocation { get; private set; }
    }
}
