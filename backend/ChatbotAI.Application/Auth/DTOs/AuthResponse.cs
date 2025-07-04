namespace ChatbotAI.Application.Auth.DTOs;

public class AuthResponse
{
    public string Token { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public string Role { get; set; } = null!;
}