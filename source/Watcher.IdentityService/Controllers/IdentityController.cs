using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Watcher.IdentityService.DataContext;
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
    private readonly ITokenService _jwtTokenService;

    public IdentityController(DatabaseContext dbContext,
        IConnectionMultiplexer connectionMultiplexer,
        IEmailService emailService,
        ITokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _redisDatabase = connectionMultiplexer.GetDatabase();
        _emailService = emailService;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("SendEmailCode")]
    public async Task<IActionResult> SendEmailCode([EmailAddress] string email)
    {
        var random = new Random();
        var confirmationCode = random.Next(100000, 999999);
        
        await _redisDatabase.StringSetAsync(email, confirmationCode, TimeSpan.FromMinutes(5));
        await _emailService.SendConfirmationCode(email, confirmationCode);
        
        return Ok("Code successfully sent");
    }
    
    [HttpPost("ConfirmCode")]
    public async Task<IActionResult> ConfirmCode([EmailAddress] string email, int code)
    {
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
        
        var token = _jwtTokenService.GenerateToken(user.Id, user.Email);
        return Ok($"{userRegistered} {token}"); // TODO: переделать возвращаемый реузльтат
    }
    
    [HttpPost("Login")]
    public async Task<IActionResult> Login([EmailAddress] string email, string password)
    {
        var user = await _dbContext.Set<User>().FirstOrDefaultAsync(user => user.Email == email);
        if (user is null)
            return Unauthorized("Invalid credentials");
    
        if (!BCrypt.Net.BCrypt.Verify(password, user.HashPassword))
            return Unauthorized("Invalid credentials");
        
        var token = _jwtTokenService.GenerateToken(user.Id, user.Email);
        return Ok(token);
    }
    
    [HttpPost("UpdatePassword")]
    public async Task<IActionResult> UpdatePassword(Guid userId, string newPassword)
    {
        var user = await _dbContext.Set<User>().FindAsync(userId);
        if (user is null)
            return NotFound("User does not exist");
        
        user.HashPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _dbContext.Set<User>().Update(user);
        await _dbContext.SaveChangesAsync();
        
        return Ok("Password successfully updated");
    }
}