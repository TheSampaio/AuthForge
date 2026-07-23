namespace Infrastructure.Persistence.Entities
{
    /// <summary>
    /// Audit trail row for a single insert or update on <c>applications</c>, written by
    /// <see cref="AppDbContext.SaveChangesWithAuditAsync"/>.
    /// </summary>
    public class ApplicationsLogEntity
    {
        public long Id { get; set; }

        public int RecordId { get; set; }

        public string? Name { get; set; }

        public Guid? ClientId { get; set; }

        public string? ClientSecret { get; set; }

        public int OperationUserId { get; set; }

        public string OperationType { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}