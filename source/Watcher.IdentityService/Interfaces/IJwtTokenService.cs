namespace Watcher.IdentityService.Interfaces;

public interface IJwtTokenService
{
    public string GenerateToken(Guid userId, string email);
}