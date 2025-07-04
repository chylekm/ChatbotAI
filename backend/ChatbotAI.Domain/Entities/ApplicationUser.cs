using Microsoft.AspNetCore.Identity;

namespace ChatbotAI.Domain.Entities;
public class ApplicationUser : IdentityUser
{
 
    public string? FullName { get; set; }
}