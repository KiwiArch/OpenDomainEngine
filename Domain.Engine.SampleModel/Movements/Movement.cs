namespace Ode.Domain.Engine.SampleModel.Movements
{
    using Locations;

    public class Movement
    {
        public MoveIn When(MovedOut movedOut)
        {
            return new MoveIn(movedOut.Movement, movedOut.ToLocation, movedOut.Item, movedOut.Location);
        }

        public void When(MovedIn movedIn)
        {
            return;
        }
    }
}
