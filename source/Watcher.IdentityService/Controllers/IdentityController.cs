using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Watcher.IdentityService.Interfaces;
using Watcher.IdentityService.Models;

namespace Watcher.IdentityService.Controllers;

[ApiController]
public class IdentityController : ControllerBase
{
    private readonly DbContext _dbContext;
    private readonly IDatabase _redisDatabase;
    private readonly IEmailService _emailService;
    private readonly IJwtTokenService _jwtTokenService;

    public IdentityController(DbContext dbContext,
        IConnectionMultiplexer connectionMultiplexer,
        IEmailService emailService,
        IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _redisDatabase = connectionMultiplexer.GetDatabase();
        _emailService = emailService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<IActionResult> SendEmailCode([EmailAddress] string email)
    {
        var random = new Random();
        var confirmationCode = random.Next(100000, 999999);
        
        await _redisDatabase.StringSetAsync(email, confirmationCode, TimeSpan.FromMinutes(5));
        await _emailService.SendConfirmationCode(email, confirmationCode);
        
        return Ok("Code successfully sent");
    }
    
    
    public async Task<IActionResult> ConfirmCode([EmailAddress] string email, int code)
    {
        return Ok();
    }
    
    public async Task<IActionResult> Login()
    {
        return Ok();
    }
    
    public async Task<IActionResult> UpdatePassword()
    {
        return Ok();
    }
}