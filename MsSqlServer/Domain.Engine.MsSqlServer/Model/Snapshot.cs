namespace Ode.Domain.Engine.MsSqlServer.Model
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Snapshot 
    {
        private const int maxColumnLength = 256;

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private set; }

        [Required, MaxLength(maxColumnLength)]
        public string SnapshotId { get; set; }

        [Required]
        public int Version { get; set; }

        [Required, MaxLength(maxColumnLength)]
        public string SnapshotType { get; set; }

        public string SnapshotData { get; set; }

        [Required, MaxLength(maxColumnLength)]
        public string CreatedBy { get; set; }

        [Required]
        public DateTime CreatedUtc { get; set; }
    }
}
