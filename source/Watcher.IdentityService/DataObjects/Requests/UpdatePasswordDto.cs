namespace Watcher.IdentityService.DataObjects.Requests;

public class UpdatePasswordDto
{
    public Guid UserId { get; set; }
    public string NewPassword { get; set; }
}