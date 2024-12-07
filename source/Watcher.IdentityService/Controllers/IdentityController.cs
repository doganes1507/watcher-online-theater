using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Watcher.IdentityService.DataContext;
using Watcher.IdentityService.DataObjects.Requests;
using Watcher.IdentityService.Interfaces;
using Watcher.IdentityService.Models;

namespace Watcher.IdentityService.Controllers;

[ApiController]
[Route("Identity")]
public class IdentityController : ControllerBase
{
    private readonly DatabaseContext _dbContext;
    private readonly IDatabase _redisDatabase;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;

    public IdentityController(DatabaseContext dbContext,
        IConnectionMultiplexer connectionMultiplexer,
        IEmailService emailService,
        ITokenService tokenService)
    {
        _dbContext = dbContext;
        _redisDatabase = connectionMultiplexer.GetDatabase();
        _emailService = emailService;
        _tokenService = tokenService;
    }

    [HttpPost("SendEmailCode")]
    public async Task<IActionResult> SendEmailCode([FromBody] SendEmailCodeDto dto)
    {
        var email = dto.Email;
        
        var random = new Random();
        var confirmationCode = random.Next(100000, 999999);
        
        await _redisDatabase.StringSetAsync(email, confirmationCode, TimeSpan.FromMinutes(5));
        await _emailService.SendConfirmationCode(email, confirmationCode);
        
        return Ok("Code successfully sent");
    }
    
    [HttpPost("ConfirmCode")]
    public async Task<IActionResult> ConfirmCode([FromBody] ConfirmCodeDto dto)
    {
        var email = dto.Email;
        var code = dto.Code;
        
        var codeFromRedis = await _redisDatabase.StringGetAsync(email);
        if (codeFromRedis != code.ToString())
        {
            return BadRequest("Invalid code");
        }
        
        await _redisDatabase.KeyDeleteAsync(email);
        
        var userRegistered = false;
        var user = await _dbContext.Set<User>().FirstOrDefaultAsync(user => user.Email == email);
        if (user is null)
        {
            user = new User { Email = email };
            await _dbContext.Set<User>().AddAsync(user);
            await _dbContext.SaveChangesAsync();
            
            // TODO: отправить запрос на добавление пользователя в AccountService
            
            userRegistered = true;
        }
        
        var token = _tokenService.GenerateToken(user.Id, user.Email);
        return Ok($"{userRegistered} {token}"); // TODO: переделать возвращаемый реузльтат
    }
    
    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var email = dto.Email;
        var password = dto.Password;
        
        var user = await _dbContext.Set<User>().FirstOrDefaultAsync(user => user.Email == email);
        if (user is null)
            return Unauthorized("Invalid credentials");
    
        if (!BCrypt.Net.BCrypt.Verify(password, user.HashPassword))
            return Unauthorized("Invalid credentials");
        
        var token = _tokenService.GenerateToken(user.Id, user.Email);
        return Ok(token);
    }
    
    [HttpPost("UpdatePassword")]
    public async Task<IActionResult> UpdatePassword([FromBody] UpdatePasswordDto dto)
    {
        var userId = dto.UserId;
        var newPassword = dto.NewPassword;
        
        var user = await _dbContext.Set<User>().FindAsync(userId);
        if (user is null)
            return NotFound("User does not exist");
        
        user.HashPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _dbContext.Set<User>().Update(user);
        await _dbContext.SaveChangesAsync();
        
        return Ok("Password successfully updated");
    }
}