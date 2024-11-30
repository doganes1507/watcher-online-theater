using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Watcher.IdentityService.Models;

public class User
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    
    [Required]
    [EmailAddress]
    [StringLength(64)]
    public string Email { get; set; }
    
    [StringLength(256)]
    public string? HashPassword { get; set; }
}