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
        if (await IsEmailExisted(registerDto.Email))
        {
            return BadRequest("Email Already Existed!");
        }

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

        return user;
    }

    private async Task<bool> IsEmailExisted(string email)
    {
        return await context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
    }
}
