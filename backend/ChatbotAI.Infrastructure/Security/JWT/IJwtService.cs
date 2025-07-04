namespace ChatbotAI.Infrastructure.Security.JWT;
public interface IJwtService
{
    string GenerateToken(string userId, string email, IEnumerable<string>? roles = null);
}