namespace Infrastructure.Persistence.Entities
{
    /// <summary>
    /// Audit trail row for a single insert or update on <c>user_applications</c>, written by
    /// <see cref="AppDbContext.SaveChangesWithAuditAsync"/>.
    /// </summary>
    public class UserApplicationsLogEntity
    {
        public long Id { get; set; }

        public int RecordId { get; set; }

        public int? UserId { get; set; }

        public int? ApplicationId { get; set; }

        public string? Roles { get; set; }

        public int OperationUserId { get; set; }

        public string OperationType { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}