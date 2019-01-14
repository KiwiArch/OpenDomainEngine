namespace Ode.Domain.Engine.SampleModel.Locations
{
    public class CreateLocation
    {
        public CreateLocation(string location, string description = null)
        {
            this.Location = location;
            this.Description = description;
        }

        public string Location { get; private set; }

        public string Description { get; private set; }
    }
}
