using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security;
using System.Security.Claims;
using System.Text;
using API.Entities;
using API.Interfaces;

using Microsoft.IdentityModel.Tokens;


namespace API.Services;

public class TokenService(IConfiguration config) : ITokenService
{
    /// <summary>
    /// (en) Creates a JWT token for the specified user using their claims and the secret key from configuration.
    /// (vi) Tạo JWT token cho user dựa trên thông tin (claims) và key bí mật trong config.
    /// </summary>
    /// <param name="user">User information (AppUser) / Thông tin người dùng</param>
    /// <returns>(en) A JWT token string / (vi) Chuỗi JWT token</returns>
    public string CreateToken(AppUser user)
    {
        // 1. Lấy secret key từ file config (appsettings.json).
        // Nếu không có thì ném lỗi ngay.
        var tokenKey = config["TokenKey"] ?? throw new SecurityTokenException("Cannot get token key!");

        // 2. Kiểm tra độ dài key phải >= 64 ký tự để đảm bảo an toàn.
        if (tokenKey.Length < 64)
        {
            throw new SecurityTokenException("Token key needs to be >= 64 characters!");
        }

        // 3. Biến chuỗi key thành SymmetricSecurityKey để dùng ký token.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));

        // 4. Tạo claims (các thông tin sẽ nhúng vào token).
        // Ví dụ: Email và Id của user.
        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id)
        };

        // 5. Tạo thông tin ký số (SigningCredentials).
        // Dùng key + thuật toán HMAC-SHA512 để ký token.
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        // 6. Định nghĩa token (token descriptor):
        // - Subject: danh tính (claims) của user
        // - Expires: thời hạn token (7 ngày)
        // - SigningCredentials: thông tin ký số
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = creds
        };

        // 7. Dùng JwtSecurityTokenHandler để tạo token từ tokenDescriptor.
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        // 8. Trả về token dưới dạng chuỗi JWT (Header.Payload.Signature).
        return tokenHandler.WriteToken(token);
    }
}
