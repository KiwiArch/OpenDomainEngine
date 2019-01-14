namespace Ode.Domain.Engine.Transactions
{
    using System;
    using System.Transactions;
    
    public static class DomainTransaction
    {
        public static TransactionScope DefaultTransactionScope()
        {
            TransactionOptions options = new TransactionOptions();

            // TODO: Read defaults from configuration
            options.Timeout = TimeSpan.FromSeconds(30);
            options.IsolationLevel = IsolationLevel.ReadCommitted;

            return new TransactionScope(TransactionScopeOption.Required, options);
        }
    }
}
