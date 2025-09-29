namespace API.Entities;

public class AppUser
{
    public String Id { get; set; } = Guid.NewGuid().ToString();
    public required String DisplayName { get; set; }
    public required String Email { get; set; }
    public required byte[] PasswordHash { get; set; }
    public required byte[] PasswordSalt { get; set; }
}
