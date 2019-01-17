namespace Ode.Domain.Engine.MsSqlServer.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using Model;
    using System;

    internal class EventContext : DbContext
    {
        private readonly string eventStoreConnectionString;
        private readonly string defaultSchema;
        private readonly string eventTableName;
        private readonly string snapshotTableName;
        private readonly string migrationTableName;
        private readonly string migrationSchema;

        public EventContext(string eventStoreConnectionString)
            : this(eventStoreConnectionString, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty)
        {
        }


        public EventContext(
           string eventStoreConnectionString,
           string defaultSchema,
           string eventTableName,
           string snapshotTableName)
           : this(eventStoreConnectionString, defaultSchema, eventTableName, snapshotTableName, string.Empty, string.Empty)
        {
        }

        public EventContext(
            string eventStoreConnectionString,
            string defaultSchema,
            string eventTableName,
            string snapshotTableName,
            string migrationSchema,
            string migrationTableName)
            : base()
        {
            this.eventStoreConnectionString = eventStoreConnectionString;
            this.defaultSchema = defaultSchema;
            this.eventTableName = eventTableName;
            this.snapshotTableName = snapshotTableName;
            this.migrationSchema = migrationSchema;
            this.migrationTableName = migrationTableName;
        }

        public DbSet<Event> Events { get; set; }

        public DbSet<Snapshot> Snapshots { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (string.IsNullOrWhiteSpace(migrationSchema) || (string.IsNullOrWhiteSpace(migrationTableName)))
            {
                optionsBuilder.UseSqlServer(eventStoreConnectionString);
            }
            else

            {
                optionsBuilder.UseSqlServer(eventStoreConnectionString, x => x.MigrationsHistoryTable(migrationTableName, migrationSchema));
            }


        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            base.OnModelCreating(modelBuilder);

            if (!string.IsNullOrWhiteSpace(defaultSchema))
            {
                modelBuilder.HasDefaultSchema(defaultSchema);
            }

            if (!string.IsNullOrWhiteSpace(eventTableName))
            {
                modelBuilder.Entity<Event>().ToTable(eventTableName);
            }

            if (!string.IsNullOrWhiteSpace(snapshotTableName))
            {
                modelBuilder.Entity<Snapshot>().ToTable(snapshotTableName);
            }

            modelBuilder.Entity<Event>(entity =>
            {
                entity.HasIndex(e => e.EventId).IsUnique();
                entity.HasIndex(e => new { e.StreamId, e.Version }).HasName("IX_StreamId_Version").IsUnique();
            });

            modelBuilder.Entity<Snapshot>(entity =>
            {
                entity.HasIndex(e => e.SnapshotId).IsUnique();
            });
        }
    }
}
