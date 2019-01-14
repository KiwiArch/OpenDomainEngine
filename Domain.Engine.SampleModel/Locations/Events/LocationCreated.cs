namespace Ode.Domain.Engine.SampleModel.Locations
{
    public class LocationCreated
    {
        public LocationCreated(string location, string description)
        {
            this.Location = location;
            this.Description = description;
        }

        public string Location { get; private set; }

        public string Description { get; private set; }
    }
}
