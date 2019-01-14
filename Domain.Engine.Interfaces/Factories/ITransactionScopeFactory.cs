namespace Ode.Domain.Engine.Factories
{
    using System.Transactions;

    public interface ITransactionScopeFactory
    {
        TransactionScope CreateTransactionScope();
    }
}
