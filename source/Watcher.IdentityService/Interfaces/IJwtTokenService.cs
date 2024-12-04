namespace Watcher.IdentityService.Interfaces;

public interface ITokenService
{
    public string GenerateToken(Guid userId, string email);
}