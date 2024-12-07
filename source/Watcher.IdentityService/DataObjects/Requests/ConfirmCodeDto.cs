namespace Watcher.IdentityService.DataObjects.Requests;

public class ConfirmCodeDto
{
    public string Email { get; set; }
    public int Code { get; set; }
}