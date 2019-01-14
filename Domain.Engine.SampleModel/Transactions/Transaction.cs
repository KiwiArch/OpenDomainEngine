namespace Ode.Domain.Engine.SampleModel.Transactions
{
    using Commands;
    using Events;

    public class Transaction
    {
        public TransactionRecorded When(RecordTransaction command)
        {
            return this.Then(new TransactionRecorded(command.TransactionIdentifier));
        }

        public TransactionRecorded Then(TransactionRecorded stateChange)
        {
            return stateChange;
        }
    }
}
