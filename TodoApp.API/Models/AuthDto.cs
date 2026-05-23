namespace TodoApp.API.Models;

public class RegisterDto
{
    public string Login { get; set; } = "";
    public string Password { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public int BirthYear { get; set; }
}

public class LoginDto
{
    public string Login { get; set; } = "";
    public string Password { get; set; } = "";
}

public class AuthResponseDto
{
    public string Token { get; set; } = "";
    public Guid UserId { get; set; }
    public string Name { get; set; } = "";
}