namespace Ode.Domain.Engine.MsSqlServer.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Event
    {
        private const int maxColumnLength = 256;

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        [Required, MaxLength(maxColumnLength)]
        public string EventId { get; set; }

        [Required, MaxLength(maxColumnLength)]
        public string StreamId { get; set; }

        [Required]
        public int Version { get; set; }

        [Required, MaxLength(maxColumnLength)]
        public string StreamType { get; set; }

        [Required, MaxLength(maxColumnLength)]
        public string EventType { get; set; }

        [MaxLength(maxColumnLength)]
        public string CommandId { get; set; }

        [Required, MaxLength(maxColumnLength)]
        public string CorrelationId { get; set; }

        public string EventData { get; set; }

        [Required, MaxLength(maxColumnLength)]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime CreatedUtc { get; set; }
    }
}
