using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class UsersEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false)]
        [MaxLength(64)]
        public string FirstName { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        [MaxLength(64)]
        public string LastName { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        [MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        [MaxLength(512)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public DateTime Birthdate { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public bool IsPlatformAdmin { get; set; }

        [Required]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
