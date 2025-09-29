using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(AppDbContext context) : BaseApiController
{
    [HttpPost("register")] //api/account/register
    public async Task<ActionResult<AppUser>> Register(RegisterDto registerDto)
    {
        if (await IsEmailExisted(registerDto.Email)) { return BadRequest("Email Already Existed!"); }

        using var hmac = new HMACSHA512();

        var user = new AppUser
        {
            DisplayName = registerDto.DisplayName,
            Email = registerDto.Email,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AppUser>> Login(LoginDto loginDto)
    {
        var user = await context.Users.SingleOrDefaultAsync(x => x.Email == loginDto.Email);

        if (user == null) { return Unauthorized("Login failed!"); }

        using var hmac = new HMACSHA512(user.PasswordSalt);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        for (var i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i]) { return Unauthorized("Login Failed!"); }
        }

        return Ok(user);
    }

    private async Task<bool> IsEmailExisted(string email)
    {
        return await context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
    }
}
