namespace Infrastructure.Persistence.Entities
{
    /// <summary>
    /// Audit trail row for a single insert or update on <c>users</c>, written by
    /// <see cref="AppDbContext.SaveChangesWithAuditAsync"/>.
    /// </summary>
    public class UsersLogEntity
    {
        public long Id { get; set; }

        public int RecordId { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }

        public string? PasswordHash { get; set; }

        public DateTime? Birthdate { get; set; }

        public int OperationUserId { get; set; }

        public string OperationType { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}