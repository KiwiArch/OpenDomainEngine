namespace Ode.Domain.Engine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;

    [Serializable]
    public class DomainOptions : HashSet<DomainOption>
    {
        protected DomainOptions(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public DomainOptions()
        {
        }

        public DomainOptions(DomainOption option)
        {
            this.Add(option);
        }

        public DomainOptions(params DomainOption[] options)
        {
            options.ToList().ForEach(o => this.Add(o));
        }

        public static DomainOptions Defaults { get { return new DomainOptions(); } }
    }
}
