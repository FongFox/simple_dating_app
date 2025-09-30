using System;
using System.Security.Cryptography;
using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

public class AccountController(AppDbContext context, ITokenService tokenService) : BaseApiController
{
    /// <summary>
    /// (en) Registers a new user account with hashed password and returns a UserDto containing token.
    /// (vi) Đăng ký tài khoản người dùng mới, mã hóa mật khẩu và trả về UserDto chứa token.
    /// </summary>
    /// <param name="registerDto">
    /// (en) The registration data including display name, email, and password.
    /// (vi) Dữ liệu đăng ký gồm tên hiển thị, email và mật khẩu.
    /// </param>
    /// <returns>
    /// (en) A UserDto object containing user info and JWT token if registration is successful.
    /// (vi) Đối tượng UserDto chứa thông tin người dùng và token nếu đăng ký thành công.
    /// </returns>
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        // 1. Kiểm tra xem email đã tồn tại trong hệ thống chưa.
        // Nếu có rồi thì trả về lỗi 400 BadRequest.
        if (await IsEmailExisted(registerDto.Email)) { return BadRequest("Email Already Existed!"); }

        // 2. Tạo đối tượng HMACSHA512 để mã hóa mật khẩu.
        // Hệ thống sẽ dùng key nội bộ (PasswordSalt) để tăng độ bảo mật.
        using var hmac = new HMACSHA512();

        // 3. Tạo đối tượng AppUser mới từ dữ liệu đăng ký.
        // - DisplayName: tên hiển thị của người dùng
        // - Email: địa chỉ email
        // - PasswordHash: mã hóa mật khẩu bằng HMACSHA512
        // - PasswordSalt: lưu key dùng để mã hóa (để xác thực sau này)
        var user = new AppUser
        {
            DisplayName = registerDto.DisplayName,
            Email = registerDto.Email,
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(registerDto.Password)),
            PasswordSalt = hmac.Key
        };

        // 4. Thêm user vào DbContext và lưu vào database.
        context.Users.Add(user);
        await context.SaveChangesAsync();

        // 5. Trả về thông tin người dùng dưới dạng UserDto (đã có token).
        // Gọi hàm ToDto(tokenService) để sinh token cho user.
        return Ok(user.ToDto(tokenService));
    }

    /// <summary>
    /// (en) Authenticates a user by verifying email and password, then returns a UserDto with JWT token.
    /// (vi) Xác thực người dùng bằng cách kiểm tra email và mật khẩu, sau đó trả về UserDto kèm token.
    /// </summary>
    /// <param name="loginDto">
    /// (en) The login credentials including email and password.
    /// (vi) Thông tin đăng nhập gồm email và mật khẩu.
    /// </param>
    /// <returns>
    /// (en) A UserDto object containing user info and JWT token if login is successful.
    /// (vi) Đối tượng UserDto chứa thông tin người dùng và token nếu đăng nhập thành công.
    /// </returns>
    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        // 1. Truy vấn người dùng từ database theo email.
        // Nếu không tìm thấy → trả về lỗi 401 Unauthorized.
        var user = await context.Users.SingleOrDefaultAsync(x => x.Email == loginDto.Email);
        if (user == null) { return Unauthorized("Login failed!"); }

        // 2. Tạo HMACSHA512 với salt đã lưu từ user.
        // Salt này chính là key đã dùng để mã hóa mật khẩu khi đăng ký.
        using var hmac = new HMACSHA512(user.PasswordSalt);

        // 3. Mã hóa lại mật khẩu người dùng vừa nhập để so sánh.
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password));

        // 4. So sánh từng byte của mật khẩu đã mã hóa với mật khẩu lưu trong DB.
        // Nếu có byte nào không khớp → trả về lỗi đăng nhập.
        for (var i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != user.PasswordHash[i]) { return Unauthorized("Login Failed!"); }
        }

        // 5. Nếu mật khẩu đúng → trả về thông tin người dùng dưới dạng UserDto (có token).
        return Ok(user.ToDto(tokenService));
    }

    private async Task<bool> IsEmailExisted(string email)
    {
        return await context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
    }
}
