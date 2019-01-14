namespace Ode.Domain.Engine.SampleModel.Transactions.Commands
{
    public class RecordTransaction
    {
        public RecordTransaction(string transactionIdentifier)
        {
            this.TransactionIdentifier = transactionIdentifier;
        }

        public string TransactionIdentifier { get; private set; }
    }
}
