namespace Ode.Domain.Engine.Tests.Model.Experimental
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public class Experiment
    {
        private int count = 0;

        public Tuple<ExperimantalEventA, ExperimantalEventB> When(PerformExperiment command)
        {
            var a = command.EmitA ? this.Then(new ExperimantalEventA(true)) : null;

            var b = command.EmitB ? this.Then(new ExperimantalEventB(this.count + 1)) : null;

            return new Tuple<ExperimantalEventA, ExperimantalEventB>(a, b);
        }

        /// <summary>
        /// This is not supported by the domain engine.
        /// </summary>
        public IEnumerable<ExperimantalEventB> When(PerformComplexExperiment command)
        {
            var results = new List<ExperimantalEventB>();

            for (int i = 0; i < command.NumberOfEvents; i++)
            {
                results.Add(this.Then(new ExperimantalEventB(i)));
            }

            return results;
        }

        public ExperimantalEventA When(PerformBadExperiment command)
        {
            if (command.ThrowException)
            {
                throw new ApplicationException();
            }

            return this.Then(new ExperimantalEventA(command.ThrowException));
        }

        public ExperimantalEventA Then(ExperimantalEventA stateChange)
        {
            return stateChange;
        }

        public ExperimantalEventB Then(ExperimantalEventB stateChange)
        {
            this.count = stateChange.Value;
            return stateChange;
        }

    }

    public class PerformExperiment
    {
        public PerformExperiment(bool emitA, bool emitB)
        {
            this.EmitA = emitA;
            this.EmitB = emitB;
        }

        public bool EmitA { get; private set; }

        public bool EmitB { get; private set; }
    }

    public class PerformComplexExperiment
    {
        public PerformComplexExperiment(int numberOfEvents)
        {
            this.NumberOfEvents = numberOfEvents;
        }

        public int NumberOfEvents { get; private set; }
    }

    public class PerformBadExperiment
    {
        public PerformBadExperiment(bool throwException)
        {
            this.ThrowException = throwException;
        }

        public bool ThrowException { get; private set; }
    }

    public class ExperimantalEventA
    {
        public ExperimantalEventA(bool outcome)
        {
            this.Outcome = outcome;
        }

        public bool Outcome { get; private set; }
    }

    public class ExperimantalEventB
    {
        public ExperimantalEventB(int value)
        {
            this.Value = value;
        }

        public int Value { get; private set; }
    }

    public class BadEvent
    {
        public BadEvent(Exception exception)
        {
            this.Exception = exception;
        }

        public Exception Exception { get; private set; }
    }
}
