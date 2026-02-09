namespace LeadBoardNet.API.Dtos.Auth;

public record RegisterRequest(string Email, string Password, string FullName);