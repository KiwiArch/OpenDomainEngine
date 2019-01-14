namespace Ode.Domain.Engine.SampleModel.Locations
{
    using System.Collections.Generic;
    using ValueObjects;

    public class Location
    {
        private string location;
        private Dictionary<string, StockItem> items = new Dictionary<string, StockItem>();

        public LocationCreated When(CreateLocation command)
        {
            var locationCreated = new LocationCreated(command.Location, command.Description);

            this.Then(locationCreated);

            return locationCreated;
        }

        public AdjustedIn When(AdjustIn command)
        {
            return this.Then(new AdjustedIn(command.Adjustment, command.Location, command.Item));
        }

        public MovedIn When(MoveIn command)
        {
            return this.Then(new MovedIn(command.Movement, command.Location, command.Item, command.FromLocation));
        }

        public MovedOut When(MoveOut command)
        {
            if (this.items.ContainsKey(command.Item.Serial))
            {
                return this.Then(new MovedOut(command.Movement, command.Location, command.Item, string.IsNullOrEmpty(command.ToLocation) ? $"InTransit\\{command.Movement}" : command.ToLocation));
            }
            else
            {
                return null;
            }
        }

        protected LocationCreated Then(LocationCreated stateChange)
        {
            this.location = stateChange.Location;
            return stateChange;
        }

        protected AdjustedIn Then(AdjustedIn stateChange)
        {
            this.items.Add(stateChange.Item.Serial, stateChange.Item);
            return stateChange;
        }

        protected MovedIn Then(MovedIn stateChange)
        {
            this.items.Add(stateChange.Item.Serial, stateChange.Item);
            return stateChange;
        }

        protected MovedOut Then(MovedOut stateChange)
        {
            this.items.Remove(stateChange.Item.Serial);
            return stateChange;
        }
    }
}
