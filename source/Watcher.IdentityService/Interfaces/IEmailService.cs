namespace Watcher.IdentityService.Interfaces;

public interface IEmailService
{
    public Task SendConfirmationCode(string to, int code);
}