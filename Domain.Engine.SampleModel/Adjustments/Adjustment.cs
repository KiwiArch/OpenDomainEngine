namespace Ode.Domain.Engine.SampleModel.Adjustments
{
    using Ode.Domain.Engine.SampleModel.Locations;
    using Transactions.Commands;

    public class Adjustment
    {
        public RecordTransaction When(AdjustedIn adjustedIn)
        {
            return new RecordTransaction($"transaction_{adjustedIn.Adjustment}");
        }
    }
}
