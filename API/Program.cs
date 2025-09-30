using System.Text;
using API.Data;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// 1. Khởi tạo builder cho ứng dụng web ASP.NET Core.
var builder = WebApplication.CreateBuilder(args);

// =======================
// 2. Đăng ký các dịch vụ vào DI container.
// =======================
// 2.1. Đăng ký controller để xử lý các endpoint HTTP.
builder.Services.AddControllers();

// 2.2. Đăng ký DbContext với SQLite.
// Lấy chuỗi kết nối từ file cấu hình (appsettings.json).
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// 2.3. Cho phép sử dụng CORS (Cross-Origin Resource Sharing).
builder.Services.AddCors();

// 2.4. Đăng ký TokenService với scope lifetime.
// Mỗi request sẽ có một instance riêng.
builder.Services.AddScoped<ITokenService, TokenService>();

// 2.5. Cấu hình xác thực JWT Bearer.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // Lấy token key từ cấu hình để xác thực chữ ký token.
                    var tokenKey = builder.Configuration["TokenKey"] ?? throw new Exception("Token key not found - Program.cs");

                    // Cấu hình các tham số xác thực token.
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true, // Kiểm tra chữ ký token
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)), // Tạo key từ chuỗi bí mật
                        ValidateIssuer = false, // Không kiểm tra issuer
                        ValidateAudience = false // Không kiểm tra audience
                    };
                });

// =======================
// 3. Build ứng dụng.
// =======================
var app = builder.Build();

// =======================
// 4. Cấu hình middleware xử lý request.
// =======================
// 4.1. Cấu hình CORS để cho phép frontend (Angular) gọi API.
// Cho phép mọi header, mọi method từ các origin cụ thể.
app.UseCors(
    options => options.AllowAnyHeader()
                      .AllowAnyMethod()
                      .WithOrigins("http://localhost:4200", "https://localhost:4200")
);

// 4.2. Kích hoạt middleware xác thực JWT.
app.UseAuthentication();

// 4.3. Kích hoạt middleware phân quyền (authorization).
app.UseAuthorization();

// 4.4. Map các controller để xử lý các route HTTP.
app.MapControllers();

// 4.5. Khởi chạy ứng dụng.
app.Run();
