using Microsoft.AspNetCore.Identity;

namespace LeadBoard.Shared.Entities;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
}