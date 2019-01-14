namespace Ode.Domain.Engine.SampleModel.Transactions.Events
{
    public class TransactionRecorded
    {
        public TransactionRecorded(string transactionIdentifier)
        {
            this.TransactionIdentifier = transactionIdentifier;
        }

        public string TransactionIdentifier { get; private set; }
    }
}
